// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Test.Fixtures;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.R.StackTracing;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Xunit;

namespace Microsoft.R.ExecutionTracing.Test {
    [ExcludeFromCodeCoverage]
    [Category.R.ExecutionTracing]
    public class SteppingTest : IAsyncLifetime {
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRSession _session;

        public SteppingTest(IServiceContainer services, TestMethodFixture testMethod) {
            _sessionProvider = new RSessionProvider(services);
            _session = _sessionProvider.GetOrCreate(testMethod.FileSystemSafeName);
        }

        public async Task InitializeAsync() {
            await _sessionProvider.TrySwitchBrokerAsync(nameof(SteppingTest));
            await _session.StartHostAsync(new RHostStartupInfo(), new RHostClientTestApp(), 50000);
        }

        public async Task DisposeAsync() {
            await _session.StopHostAsync();
            _sessionProvider.Dispose();
        }

        [Test(Skip = "https://github.com/Microsoft/RTVS/issues/2009")]
        public async Task BreakContinue() {
            const string code =
@"x <- 0
  browser()
  while (x >= 0) {
    x <- x + 1
  }
  browser()";

            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile(code)) {
                await sf.Source(_session);
                await _session.NextPromptShouldBeBrowseAsync();

                await tracer.ContinueAsync();
                await Task.Delay(100);
                await tracer.BreakAsync();

                await _session.NextPromptShouldBeBrowseAsync();

                var frame = (await _session.TracebackAsync()).Single();
                frame.FileName.Should().Be(sf.FilePath);
                frame.LineNumber.Should().BeInRange(3, 5);

                await _session.ExecuteAsync("x <- -42");
                await tracer.ContinueAsync();

                await _session.NextPromptShouldBeBrowseAsync();
                await _session.ShouldHaveTracebackAsync(new TracebackBuilder {
                    { sf, 6 }
                });
            }
        }

        [Test]
        public async Task StepOver() {
            const string code =
@"f <- function(x) {
    x + 1
  }
  x <- f(1)
  print(x)";

            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile(code)) {
                await tracer.EnableBreakpointsAsync(true);

                var bp = await tracer.CreateBreakpointAsync(sf, 4);
                var bpHit = new BreakpointHitDetector(bp);

                await sf.Source(_session);
                await bpHit.ShouldBeHitAtNextPromptAsync();
                await _session.ShouldHaveTracebackAsync(new TracebackBuilder {
                    { bp.Location }
                });

                (await tracer.StepOverAsync()).Should().Be(true);
                await _session.ShouldHaveTracebackAsync(new TracebackBuilder {
                    { bp.Location, +1 }
                });
            }
        }

        [Test]
        [Category.R.ExecutionTracing]
        public async Task StepInto() {
            const string code =
@"f <- function(x) {
    x + 1
  }
  x <- f(1)
  print(x)";

            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile(code)) {
                await tracer.EnableBreakpointsAsync(true);

                var bp = await tracer.CreateBreakpointAsync(sf, 4);
                var bpHit = new BreakpointHitDetector(bp);

                await sf.Source(_session);
                await bpHit.ShouldBeHitAtNextPromptAsync();
                await _session.ShouldHaveTracebackAsync(new TracebackBuilder {
                    { bp.Location }
                });

                (await tracer.StepIntoAsync()).Should().Be(true);
                await _session.ShouldHaveTracebackAsync(new TracebackBuilder {
                    { sf, 4, "f(1)" },
                    { sf, 1, TracebackBuilder.Any },
                });
            }
        }

        [Test(Skip = "https://github.com/Microsoft/RTVS/issues/975")]
        public async Task StepOutToGlobal() {
            const string code =
@"f <- function(x) {
    x + 1
  }
  x <- f(1)
  print(x)";

            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile(code)) {
                await tracer.EnableBreakpointsAsync(true);

                var bp = await tracer.CreateBreakpointAsync(sf, 2);
                var bpHit = new BreakpointHitDetector(bp);

                await sf.Source(_session);
                await bpHit.ShouldBeHitAtNextPromptAsync();
                await _session.ShouldHaveTracebackAsync(new TracebackBuilder {
                    { bp.Location, "f(1)" }
                });

                (await tracer.StepOutAsync()).Should().Be(true);
                await _session.ShouldHaveTracebackAsync(new TracebackBuilder {
                    { sf, 5 }
                });
            }
        }

        [Test]
        public async Task StepOutToFunction() {
            const string code =
@"f <- function() {
    1
  }
  g <- function() {
    f()
    1
  }
  g()";

            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile(code)) {
                await tracer.EnableBreakpointsAsync(true);

                var bp = await tracer.CreateBreakpointAsync(sf, 2);
                var bpHit = new BreakpointHitDetector(bp);

                await sf.Source(_session);
                await bpHit.ShouldBeHitAtNextPromptAsync();
                await _session.ShouldHaveTracebackAsync(new TracebackBuilder {
                    { sf, 8, "g()" },
                    { sf, 5, "f()" },
                    { bp.Location },
                });

                (await tracer.StepOutAsync()).Should().Be(true);
                await _session.ShouldHaveTracebackAsync(new TracebackBuilder {
                    { sf, 8, "g()" },
                    { sf, 6, TracebackBuilder.Any },
                });
            }
        }

        [Test]
        public async Task StepOutFromGlobal() {
            const string code =
@"x <- 1
  y <- 2";

            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile(code)) {
                await tracer.EnableBreakpointsAsync(true);

                var bp1 = await tracer.CreateBreakpointAsync(sf, 1);
                var bp2 = await tracer.CreateBreakpointAsync(sf, 2);

                var bp1Hit = new BreakpointHitDetector(bp1);

                await sf.Source(_session);
                await bp1Hit.ShouldBeHitAtNextPromptAsync();
                await _session.ShouldHaveTracebackAsync(new TracebackBuilder {
                    { bp1.Location }
                });

                (await tracer.StepOutAsync()).Should().Be(false);
                await _session.ShouldHaveTracebackAsync(new TracebackBuilder {
                    { bp2.Location }
                });
            }
        }

        [Test]
        public async Task StepOverBreakpoint() {
            const string code =
@"f <- function() { 
    0
  }
  x <- f()
  z <- 3";

            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile(code)) {
                await tracer.EnableBreakpointsAsync(true);

                var bp1 = await tracer.CreateBreakpointAsync(sf, 4);
                var bp2 = await tracer.CreateBreakpointAsync(sf, 2);

                var bp1Hit = new BreakpointHitDetector(bp1);
                var bp2Hit = new BreakpointHitDetector(bp2);

                await sf.Source(_session);
                await bp1Hit.ShouldBeHitAtNextPromptAsync();

                (await tracer.StepOverAsync()).Should().Be(false);
                await bp2Hit.ShouldBeHitAtNextPromptAsync();
            }
        }

        [Test]
        public async Task StepOntoBreakpoint() {
            const string code =
@"x <- 1
  y <- 2";

            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile(code)) {
                await tracer.EnableBreakpointsAsync(true);

                var bp1 = await tracer.CreateBreakpointAsync(sf, 1);
                var bp2 = await tracer.CreateBreakpointAsync(sf, 2);

                var bp1Hit = new BreakpointHitDetector(bp1);
                var bp2Hit = new BreakpointHitDetector(bp2);

                await sf.Source(_session);
                await bp1Hit.ShouldBeHitAtNextPromptAsync();

                (await tracer.StepOverAsync()).Should().Be(false);
                await bp2Hit.ShouldBeHitAtNextPromptAsync();
            }
        }


        [Test]
        public async Task StepIntoAfterStepOver() {
            const string code =
@"f <- function(x) {
    x + 1
  }
  x <- 1
  x <- f(1)
  print(x)";

            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile(code)) {
                await tracer.EnableBreakpointsAsync(true);

                var bp = await tracer.CreateBreakpointAsync(sf, 4);
                var bpHit = new BreakpointHitDetector(bp);

                await sf.Source(_session);
                await bpHit.ShouldBeHitAtNextPromptAsync();

                (await tracer.StepOverAsync()).Should().BeTrue();
                await _session.ShouldBeAtAsync(bp.Location, +1);

                (await tracer.StepIntoAsync()).Should().BeTrue();
                await _session.ShouldBeAtAsync(sf.FilePath, 1);
            }
        }
    }
}

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
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.UnitTests.Core.XUnit.MethodFixtures;
using Xunit;

namespace Microsoft.R.ExecutionTracing.Test {
    [ExcludeFromCodeCoverage]
    [Category.R.ExecutionTracing]
    public class BreakpointsTest : IAsyncLifetime {
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRSession _session;

        public BreakpointsTest(IServiceContainer services, TestMethodFixture testMethod) {
            _sessionProvider = new RSessionProvider(services);
            _session = _sessionProvider.GetOrCreate(testMethod.FileSystemSafeName);
        }

        public async Task InitializeAsync() {
            await _sessionProvider.TrySwitchBrokerAsync(nameof(BreakpointsTest));
            await _session.StartHostAsync(new RHostStartupInfo(isInteractive: true), new RHostClientTestApp(), 50000);
        }

        public async Task DisposeAsync() {
            await _session.StopHostAsync();
            _sessionProvider.Dispose();
        }

        [Test]
        public async Task AddRemoveBreakpoint() {
            const string code =
@"x <- 1
  y <- 2
  z <- 3";

            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile(code)) {
                var bp1Loc = new RSourceLocation(sf.FilePath, 1);
                var bp1 = await tracer.CreateBreakpointAsync(bp1Loc);

                bp1.Location.Should().Be(bp1Loc);
                bp1.Tracer.Should().Be(tracer);

                tracer.Breakpoints.Count.Should().Be(1);

                var bp2Loc = new RSourceLocation(sf.FilePath, 3);
                var bp2 = await tracer.CreateBreakpointAsync(bp2Loc);

                bp2.Location.Should().Be(bp2Loc);
                bp2.Tracer.Should().Be(tracer);

                tracer.Breakpoints.Count.Should().Be(2);

                await bp1.DeleteAsync();
                tracer.Breakpoints.Count.Should().Be(1);
                tracer.Breakpoints.First().Should().BeSameAs(bp2);
            }

        }

        [Test]
        public async Task SetAndHitToplevelBreakpoint() {
            const string code =
@"x <- 1
  y <- 2
  z <- 3";

            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile(code)) {
                await tracer.EnableBreakpointsAsync(true);

                var bp = await tracer.CreateBreakpointAsync(new RSourceLocation(sf.FilePath, 2));
                var bpHit = new BreakpointHitDetector(bp);

                await sf.Source(_session);
                await bpHit.ShouldBeHitAtNextPromptAsync();
            }
        }

        [Test]
        [Category.R.ExecutionTracing]
        public async Task SetAndHitBreakpointInsideUnloadedFunction() {
            const string code =
@"f <- function() {
    0
  }
  f()";

            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile(code)) {
                await tracer.EnableBreakpointsAsync(true);

                var bp = await tracer.CreateBreakpointAsync(new RSourceLocation(sf.FilePath, 2));
                var bpHit = new BreakpointHitDetector(bp);

                await sf.Source(_session);
                await bpHit.ShouldBeHitAtNextPromptAsync();
            }
        }

        [Test]
        public async Task SetAndHitBreakpointInsideLoadedFunction() {
            const string code =
@"f <- function() {
    0
  }";

            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile(code)) {
                await tracer.EnableBreakpointsAsync(true);
                await sf.Source(_session);

                var bp = await tracer.CreateBreakpointAsync(new RSourceLocation(sf.FilePath, 2));
                var bpHit = new BreakpointHitDetector(bp);

                using (var inter = await _session.BeginInteractionAsync()) {
                    await inter.RespondAsync("f()\n");
                }

                await bpHit.ShouldBeHitAtNextPromptAsync();
            }
        }

        [Test]
        public async Task RemovedBreakpointNotHit() {
            const string code =
@"f <- function() {
    0
  }";

            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile(code)) {
                await tracer.EnableBreakpointsAsync(true);
                await sf.Source(_session);

                var bp = await tracer.CreateBreakpointAsync(new RSourceLocation(sf.FilePath, 2));
                var bpHit = new BreakpointHitDetector(bp);

                using (var inter = await _session.BeginInteractionAsync()) {
                    await inter.RespondAsync("f()\n");
                }

                await bpHit.ShouldBeHitAtNextPromptAsync();

                await bp.DeleteAsync();
                await tracer.ContinueAsync();

                using (var inter = await _session.BeginInteractionAsync()) {
                    await inter.RespondAsync("f()\n");
                }

                using (var inter = await _session.BeginInteractionAsync()) {
                    inter.Contexts.IsBrowser().Should().BeFalse();
                }
            }
        }

        [Test]
        public async Task SetBreakpointOnNull() {
            const string code =
@"f <- function() {
    NULL
  }";

            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile(code)) {
                var bp = await tracer.CreateBreakpointAsync(new RSourceLocation(sf.FilePath, 2));
                tracer.Breakpoints.Count.Should().Be(1);

                await sf.Source(_session);

                (await _session.EvaluateAsync<bool>("is.function(f)", REvaluationKind.Normal)).Should().BeTrue();
            }
        }

        [Test]
        public async Task OverlappingBreakpoints() {
            const string code =
@"f <- function() {
    1
  }";

            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile(code)) {
                await tracer.EnableBreakpointsAsync(true);
                await sf.Source(_session);

                var bp1 = await tracer.CreateBreakpointAsync(sf, 1);
                var bp2 = await tracer.CreateBreakpointAsync(sf, 1);

                bp1.Should().BeSameAs(bp2);
                tracer.Breakpoints.Should().HaveCount(1);

                var bp1Hit = new BreakpointHitDetector(bp1);
                var bp2Hit = new BreakpointHitDetector(bp2);

                using (var inter = await _session.BeginInteractionAsync()) {
                    await inter.RespondAsync("f()\n");
                }

                await bp1Hit.ShouldBeHitAtNextPromptAsync();
                bp2Hit.WasHit.Should().BeTrue();

                await bp1.DeleteAsync();
                tracer.Breakpoints.Should().HaveCount(1);
                tracer.Breakpoints.Should().Contain(bp2);

                await tracer.ContinueAsync();

                using (var inter = await _session.BeginInteractionAsync()) {
                    await inter.RespondAsync("f()\n");
                }

                await bp2Hit.ShouldBeHitAtNextPromptAsync();

                await bp2.DeleteAsync();
                tracer.Breakpoints.Should().BeEmpty();

                await tracer.ContinueAsync();

                using (var inter = await _session.BeginInteractionAsync()) {
                    await inter.RespondAsync("f()\n");
                }

                using (var inter = await _session.BeginInteractionAsync()) {
                    inter.Contexts.IsBrowser().Should().BeFalse();
                }
            }
        }

        [Test]
        public async Task BreakpointsInDifferentFiles() {
            var tracer = await _session.TraceExecutionAsync();
            using (var sf1 = new SourceFile("1"))
            using (var sf2 = new SourceFile($"eval(parse({sf1.FilePath.ToRStringLiteral()}))")) {
                await tracer.EnableBreakpointsAsync(true);

                var bp1Loc = new RSourceLocation(sf1.FilePath, 1);
                var bp1 = await tracer.CreateBreakpointAsync(bp1Loc);
                bp1.Location.Should().Be(bp1Loc);

                var bp2Loc = new RSourceLocation(sf2.FilePath, 1);
                var bp2 = await tracer.CreateBreakpointAsync(bp2Loc);
                bp2.Location.Should().Be(bp2Loc);

                tracer.Breakpoints.Should().HaveCount(2);

                var bp1Hit = new BreakpointHitDetector(bp1);
                var bp2Hit = new BreakpointHitDetector(bp2);

                await sf2.Source(_session);

                await bp2Hit.ShouldBeHitAtNextPromptAsync();
                bp1Hit.WasHit.Should().BeFalse();

                bp2Hit.Reset();
                await tracer.ContinueAsync();

                await bp1Hit.ShouldBeHitAtNextPromptAsync();
                bp2Hit.WasHit.Should().BeFalse();
            }
        }

        [Test]
        public async Task SetBreakpointWhileRunning() {
            const string code =
@"browser()
  f <- function() {
    NULL
  }
  while (TRUE) f()";

            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile(code)) {
                await tracer.EnableBreakpointsAsync(true);

                await sf.Source(_session);
                await _session.NextPromptShouldBeBrowseAsync();
                await tracer.ContinueAsync();
                await Task.Delay(100);

                var bp = await tracer.CreateBreakpointAsync(sf, 3);

                await _session.NextPromptShouldBeBrowseAsync();
                await _session.ShouldBeAtAsync(bp.Location);
            }
        }

        [Test]
        public async Task RemoveBreakpointWhileRunning() {
            const string code =
@"browser()
  f <- function() {
    NULL
    browser()
  }
  b <- FALSE;
  while (TRUE) if (b) f()";

            var tracer = await _session.TraceExecutionAsync();
            using (var sf = new SourceFile(code)) {
                await tracer.EnableBreakpointsAsync(true);

                await sf.Source(_session);
                await _session.NextPromptShouldBeBrowseAsync();

                var bp = await tracer.CreateBreakpointAsync(sf, 3);
                int hitCount = 0;
                bp.BreakpointHit += delegate { ++hitCount; };

                await tracer.ContinueAsync();
                await Task.Delay(100);

                await bp.DeleteAsync();
                await _session.ExecuteAsync("b <- TRUE");

                await _session.NextPromptShouldBeBrowseAsync();

                await _session.ShouldBeAtAsync(sf.FilePath, 4);
                hitCount.Should().Be(0);
            }
        }

        [Test]
        public async Task BrowseOnNewPrompt() {
            var tracer = await _session.TraceExecutionAsync();
            var browseTask = EventTaskSources.IRExecutionTracer.Browse.Create(tracer);

            using (var inter = await _session.BeginInteractionAsync()) {
                await inter.RespondAsync("browser()\n");
            }

            await _session.NextPromptShouldBeBrowseAsync();
            await browseTask;
        }

        [Test]
        [Category.R.ExecutionTracing]
        public async Task BrowseOnExistingPrompt() {
            using (var inter = await _session.BeginInteractionAsync()) {
                await inter.RespondAsync("browser()\n");
            }

            using (var inter = await _session.BeginInteractionAsync()) {
                inter.Contexts.IsBrowser().Should().BeTrue();
            }

            var tracer = await _session.TraceExecutionAsync();
            await EventTaskSources.IRExecutionTracer.Browse.Create(tracer);
        }
    }
}

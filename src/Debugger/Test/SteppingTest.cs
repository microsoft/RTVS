// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Debugger.Test {
    [ExcludeFromCodeCoverage]
    public class SteppingTest : IAsyncLifetime {
        private readonly MethodInfo _testMethod;
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRSession _session;

        public SteppingTest(TestMethodFixture testMethod) {
            _testMethod = testMethod.MethodInfo;
            _sessionProvider = new RSessionProvider();
            _session = _sessionProvider.GetOrCreate(Guid.NewGuid(), new RHostClientTestApp());
        }

        public async Task InitializeAsync() {
            await _session.StartHostAsync(new RHostStartupInfo {
                Name = _testMethod.Name,
                RBasePath = RUtilities.FindExistingRBasePath()
            }, 50000);
        }

        public async Task DisposeAsync() {
            await _session.StopHostAsync();
            _sessionProvider.Dispose();
        }

        [Test]
        [Category.R.Debugger]
        public async Task BreakContinue() {
            const string code =
@"browser()
  x <- 0
  while (x >= 0) {
    x <- x + 1
  }
  browser()";

            using (var debugSession = new DebugSession(_session))
            using (var sf = new SourceFile(code)) {
                await sf.Source(_session);
                await debugSession.NextPromptShouldBeBrowseAsync();

                await debugSession.ContinueAsync();
                await Task.Delay(100);
                await debugSession.BreakAsync();

                await debugSession.NextPromptShouldBeBrowseAsync();
                (await debugSession.GetStackFramesAsync()).Should().HaveTail(new MatchDebugStackFrames {
                    { sf, new MatchRange<int>(1, 3) }
                });

                await _session.EvaluateAsync("x <- -42", REvaluationKind.Normal);
                await debugSession.ContinueAsync();

                await debugSession.NextPromptShouldBeBrowseAsync();
                (await debugSession.GetStackFramesAsync()).Should().HaveTail(new MatchDebugStackFrames {
                    { sf, 6 }
                });
            }
        }

        [Test]
        [Category.R.Debugger]
        public async Task StepOver() {
            const string code =
@"f <- function(x) {
    x + 1
  }
  x <- f(1)
  print(x)";

            using (var debugSession = new DebugSession(_session))
            using (var sf = new SourceFile(code)) {
                await debugSession.EnableBreakpointsAsync(true);

                var bp = await debugSession.CreateBreakpointAsync(sf, 4);
                var bpHit = new BreakpointHitDetector(bp);

                await sf.Source(_session);
                await bpHit.ShouldBeHitAtNextPromptAsync();
                (await debugSession.GetStackFramesAsync()).Should().HaveTail(new MatchDebugStackFrames {
                    { bp.Location }
                });

                (await debugSession.StepOverAsync()).Should().Be(true);
                (await debugSession.GetStackFramesAsync()).Should().HaveTail(new MatchDebugStackFrames {
                    { bp.Location, +1 }
                });
            }
        }

        [Test]
        [Category.R.Debugger]
        public async Task StepInto() {
            const string code =
@"f <- function(x) {
    x + 1
  }
  x <- f(1)
  print(x)";

            using (var debugSession = new DebugSession(_session))
            using (var sf = new SourceFile(code)) {
                await debugSession.EnableBreakpointsAsync(true);

                var bp = await debugSession.CreateBreakpointAsync(sf, 4);
                var bpHit = new BreakpointHitDetector(bp);

                await sf.Source(_session);
                await bpHit.ShouldBeHitAtNextPromptAsync();
                (await debugSession.GetStackFramesAsync()).Should().HaveTail(new MatchDebugStackFrames {
                    { bp.Location }
                });

                (await debugSession.StepIntoAsync()).Should().Be(true);
                (await debugSession.GetStackFramesAsync()).Should().HaveTail(new MatchDebugStackFrames {
                    { sf, 4, "f(1)" },
                    { sf, 1, MatchAny<string>.Instance },
                });
            }
        }

        [Test(Skip = "https://github.com/Microsoft/RTVS/issues/975")]
        [Category.R.Debugger]
        public async Task StepOutToGlobal() {
            const string code =
@"f <- function(x) {
    x + 1
  }
  x <- f(1)
  print(x)";

            using (var debugSession = new DebugSession(_session))
            using (var sf = new SourceFile(code)) {
                await debugSession.EnableBreakpointsAsync(true);

                var bp = await debugSession.CreateBreakpointAsync(sf, 2);
                var bpHit = new BreakpointHitDetector(bp);

                await sf.Source(_session);
                await bpHit.ShouldBeHitAtNextPromptAsync();
                (await debugSession.GetStackFramesAsync()).Should().HaveTail(new MatchDebugStackFrames {
                    { bp.Location, "f(1)" }
                });

                (await debugSession.StepOutAsync()).Should().Be(true);
                (await debugSession.GetStackFramesAsync()).Should().HaveTail(new MatchDebugStackFrames {
                    { sf, 5 }
                });
            }
        }

        [Test]
        [Category.R.Debugger]
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

            using (var debugSession = new DebugSession(_session))
            using (var sf = new SourceFile(code)) {
                await debugSession.EnableBreakpointsAsync(true);

                var bp = await debugSession.CreateBreakpointAsync(sf, 2);
                var bpHit = new BreakpointHitDetector(bp);

                await sf.Source(_session);
                await bpHit.ShouldBeHitAtNextPromptAsync();
                (await debugSession.GetStackFramesAsync()).Should().HaveTail(new MatchDebugStackFrames {
                    { sf, 8, "g()" },
                    { sf, 5, "f()" },
                    { bp.Location },
                });

                (await debugSession.StepOutAsync()).Should().Be(true);
                (await debugSession.GetStackFramesAsync()).Should().HaveTail(new MatchDebugStackFrames {
                    { sf, 8, "g()" },
                    { sf, 6, MatchAny<string>.Instance },
                });
            }
        }

        [Test]
        [Category.R.Debugger]
        public async Task StepOutFromGlobal() {
            const string code =
@"x <- 1
  y <- 2";

            using (var debugSession = new DebugSession(_session))
            using (var sf = new SourceFile(code)) {
                await debugSession.EnableBreakpointsAsync(true);

                var bp1 = await debugSession.CreateBreakpointAsync(sf, 1);
                var bp2 = await debugSession.CreateBreakpointAsync(sf, 2);

                var bp1Hit = new BreakpointHitDetector(bp1);

                await sf.Source(_session);
                await bp1Hit.ShouldBeHitAtNextPromptAsync();
                (await debugSession.GetStackFramesAsync()).Should().HaveTail(new MatchDebugStackFrames {
                    { bp1.Location }
                });

                (await debugSession.StepOutAsync()).Should().Be(false);
                (await debugSession.GetStackFramesAsync()).Should().HaveTail(new MatchDebugStackFrames {
                    { bp2.Location }
                });
            }
        }

        [Test]
        [Category.R.Debugger]
        public async Task StepOverBreakpoint() {
            const string code =
@"f <- function() { 
    0
  }
  x <- f()
  z <- 3";

            using (var debugSession = new DebugSession(_session))
            using (var sf = new SourceFile(code)) {
                await debugSession.EnableBreakpointsAsync(true);

                var bp1 = await debugSession.CreateBreakpointAsync(sf, 4);
                var bp2 = await debugSession.CreateBreakpointAsync(sf, 2);

                var bp1Hit = new BreakpointHitDetector(bp1);
                var bp2Hit = new BreakpointHitDetector(bp2);

                await sf.Source(_session);
                await bp1Hit.ShouldBeHitAtNextPromptAsync();

                (await debugSession.StepOverAsync()).Should().Be(false);
                await bp2Hit.ShouldBeHitAtNextPromptAsync();
            }
        }

        [Test]
        [Category.R.Debugger]
        public async Task StepOntoBreakpoint() {
            const string code =
@"x <- 1
  y <- 2";

            using (var debugSession = new DebugSession(_session))
            using (var sf = new SourceFile(code)) {
                await debugSession.EnableBreakpointsAsync(true);

                var bp1 = await debugSession.CreateBreakpointAsync(sf, 1);
                var bp2 = await debugSession.CreateBreakpointAsync(sf, 2);

                var bp1Hit = new BreakpointHitDetector(bp1);
                var bp2Hit = new BreakpointHitDetector(bp2);

                await sf.Source(_session);
                await bp1Hit.ShouldBeHitAtNextPromptAsync();

                (await debugSession.StepOverAsync()).Should().Be(false);
                await bp2Hit.ShouldBeHitAtNextPromptAsync();
            }
        }


        [Test]
        [Category.R.Debugger]
        public async Task StepIntoAfterStepOver() {
            const string code =
@"f <- function(x) {
    x + 1
  }
  x <- 1
  x <- f(1)
  print(x)";

            using (var debugSession = new DebugSession(_session))
            using (var sf = new SourceFile(code)) {
                await debugSession.EnableBreakpointsAsync(true);

                var bp = await debugSession.CreateBreakpointAsync(sf, 4);
                var bpHit = new BreakpointHitDetector(bp);

                await sf.Source(_session);
                await bpHit.ShouldBeHitAtNextPromptAsync();

                (await debugSession.StepOverAsync()).Should().BeTrue();
                (await debugSession.GetStackFramesAsync()).Should().HaveTail(new MatchDebugStackFrames {
                    { bp.Location, +1 }
                });

                (await debugSession.StepIntoAsync()).Should().BeTrue();
                (await debugSession.GetStackFramesAsync()).Should().HaveTail(new MatchDebugStackFrames {
                    { sf, 1 }
                });
            }
        }
    }
}

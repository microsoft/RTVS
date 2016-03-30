// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Tasks;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Debugger.Test {
    [ExcludeFromCodeCoverage]
    public class BreakpointsTest : IAsyncLifetime {
        private readonly MethodInfo _testMethod;
        private readonly RSessionProvider _sessionProvider;
        private readonly IRSession _session;

        public BreakpointsTest(TestMethodFixture testMethod) {
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
        public async Task AddRemoveBreakpoint() {
            const string code =
@"x <- 1
  y <- 2
  z <- 3";

            using (var debugSession = new DebugSession(_session))
            using (var sf = new SourceFile(code)) {
                var bp1Loc = new DebugBreakpointLocation(sf.FilePath, 1);
                var bp1 = await debugSession.CreateBreakpointAsync(bp1Loc);

                bp1.Location.Should().Be(bp1Loc);
                bp1.Session.Should().Be(debugSession);

                debugSession.Breakpoints.Count.Should().Be(1);

                var bp2Loc = new DebugBreakpointLocation(sf.FilePath, 3);
                var bp2 = await debugSession.CreateBreakpointAsync(bp2Loc);

                bp2.Location.Should().Be(bp2Loc);
                bp2.Session.Should().Be(debugSession);

                debugSession.Breakpoints.Count.Should().Be(2);

                await bp1.DeleteAsync();
                debugSession.Breakpoints.Count.Should().Be(1);
                debugSession.Breakpoints.First().Should().BeSameAs(bp2);
            }

        }

        [Test]
        [Category.R.Debugger]
        public async Task SetAndHitToplevelBreakpoint() {
            const string code =
@"x <- 1
  y <- 2
  z <- 3";

            using (var debugSession = new DebugSession(_session))
            using (var sf = new SourceFile(code)) {
                await debugSession.EnableBreakpointsAsync(true);

                var bp = await debugSession.CreateBreakpointAsync(new DebugBreakpointLocation(sf.FilePath, 2));
                var bpHit = new BreakpointHitDetector(bp);

                await sf.Source(_session);
                await bpHit.ShouldBeHitAtNextPromptAsync();
            }
        }

        [Test]
        [Category.R.Debugger]
        public async Task SetAndHitBreakpointInsideUnloadedFunction() {
            const string code =
@"f <- function() {
    0
  }
  f()";

            using (var debugSession = new DebugSession(_session))
            using (var sf = new SourceFile(code)) {
                await debugSession.EnableBreakpointsAsync(true);

                var bp = await debugSession.CreateBreakpointAsync(new DebugBreakpointLocation(sf.FilePath, 2));
                var bpHit = new BreakpointHitDetector(bp);

                await sf.Source(_session);
                await bpHit.ShouldBeHitAtNextPromptAsync();
            }
        }

        [Test]
        [Category.R.Debugger]
        public async Task SetAndHitBreakpointInsideLoadedFunction() {
            const string code =
@"f <- function() {
    0
  }";

            using (var debugSession = new DebugSession(_session))
            using (var sf = new SourceFile(code)) {
                await debugSession.EnableBreakpointsAsync(true);
                await sf.Source(_session);

                var bp = await debugSession.CreateBreakpointAsync(new DebugBreakpointLocation(sf.FilePath, 2));
                var bpHit = new BreakpointHitDetector(bp);

                using (var inter = await _session.BeginInteractionAsync()) {
                    await inter.RespondAsync("f()\n");
                }

                await bpHit.ShouldBeHitAtNextPromptAsync();
            }
        }

        [Test]
        [Category.R.Debugger]
        public async Task RemovedBreakpointNotHit() {
            const string code =
@"f <- function() {
    0
  }";

            using (var debugSession = new DebugSession(_session))
            using (var sf = new SourceFile(code)) {
                await debugSession.EnableBreakpointsAsync(true);
                await sf.Source(_session);

                var bp = await debugSession.CreateBreakpointAsync(new DebugBreakpointLocation(sf.FilePath, 2));
                var bpHit = new BreakpointHitDetector(bp);

                using (var inter = await _session.BeginInteractionAsync()) {
                    await inter.RespondAsync("f()\n");
                }

                await bpHit.ShouldBeHitAtNextPromptAsync();

                await bp.DeleteAsync();
                await debugSession.ContinueAsync();

                using (var inter = await _session.BeginInteractionAsync()) {
                    await inter.RespondAsync("f()\n");
                }

                using (var inter = await _session.BeginInteractionAsync()) {
                    inter.Contexts.IsBrowser().Should().BeFalse();
                }
            }
        }

        [Test]
        [Category.R.Debugger]
        public async Task SetBreakpointOnNull() {
            const string code =
@"f <- function() {
    NULL
  }";

            using (var debugSession = new DebugSession(_session))
            using (var sf = new SourceFile(code)) {
                var bp = await debugSession.CreateBreakpointAsync(new DebugBreakpointLocation(sf.FilePath, 2));
                debugSession.Breakpoints.Count.Should().Be(1);

                await sf.Source(_session);

                var res = (await debugSession.EvaluateAsync("is.function(f)", DebugEvaluationResultFields.ReprDeparse)).As<DebugValueEvaluationResult>();
                res.GetRepresentation().Deparse
                    .Should().Be("TRUE");
            }
        }

        [Test]
        [Category.R.Debugger]
        public async Task OverlappingBreakpoints() {
            const string code =
@"f <- function() {
    1
  }";

            using (var debugSession = new DebugSession(_session))
            using (var sf = new SourceFile(code)) {
                await debugSession.EnableBreakpointsAsync(true);
                await sf.Source(_session);

                var bp1 = await debugSession.CreateBreakpointAsync(sf, 1);
                var bp2 = await debugSession.CreateBreakpointAsync(sf, 1);

                bp1.Should().BeSameAs(bp2);
                debugSession.Breakpoints.Should().HaveCount(1);

                var bp1Hit = new BreakpointHitDetector(bp1);
                var bp2Hit = new BreakpointHitDetector(bp2);

                using (var inter = await _session.BeginInteractionAsync()) {
                    await inter.RespondAsync("f()\n");
                }

                await bp1Hit.ShouldBeHitAtNextPromptAsync();
                bp2Hit.WasHit.Should().BeTrue();

                await bp1.DeleteAsync();
                debugSession.Breakpoints.Should().HaveCount(1);
                debugSession.Breakpoints.Should().Contain(bp2);

                await debugSession.ContinueAsync();

                using (var inter = await _session.BeginInteractionAsync()) {
                    await inter.RespondAsync("f()\n");
                }

                await bp2Hit.ShouldBeHitAtNextPromptAsync();

                await bp2.DeleteAsync();
                debugSession.Breakpoints.Should().HaveCount(0);

                await debugSession.ContinueAsync();

                using (var inter = await _session.BeginInteractionAsync()) {
                    await inter.RespondAsync("f()\n");
                }

                using (var inter = await _session.BeginInteractionAsync()) {
                    inter.Contexts.IsBrowser().Should().BeFalse();
                }
            }
        }

        [Test]
        [Category.R.Debugger]
        public async Task BreakpointsInDifferentFiles() {
            using (var debugSession = new DebugSession(_session))
            using (var sf1 = new SourceFile("1"))
            using (var sf2 = new SourceFile($"eval(parse({sf1.FilePath.ToRStringLiteral()}))")) {
                await debugSession.EnableBreakpointsAsync(true);

                var bp1Loc = new DebugBreakpointLocation(sf1.FilePath, 1);
                var bp1 = await debugSession.CreateBreakpointAsync(bp1Loc);
                bp1.Location.Should().Be(bp1Loc);

                var bp2Loc = new DebugBreakpointLocation(sf2.FilePath, 1);
                var bp2 = await debugSession.CreateBreakpointAsync(bp2Loc);
                bp2.Location.Should().Be(bp2Loc);

                debugSession.Breakpoints.Should().HaveCount(2);

                var bp1Hit = new BreakpointHitDetector(bp1);
                var bp2Hit = new BreakpointHitDetector(bp2);

                await sf2.Source(_session);

                await bp2Hit.ShouldBeHitAtNextPromptAsync();
                bp1Hit.WasHit.Should().BeFalse();

                bp2Hit.Reset();
                await debugSession.ContinueAsync();

                await bp1Hit.ShouldBeHitAtNextPromptAsync();
                bp2Hit.WasHit.Should().BeFalse();
            }
        }

        [Test]
        [Category.R.Debugger]
        public async Task SetBreakpointWhileRunning() {
            const string code =
@"browser()
  f <- function() {
    NULL
  }
  while (TRUE) f()";

            using (var debugSession = new DebugSession(_session))
            using (var sf = new SourceFile(code)) {
                await debugSession.EnableBreakpointsAsync(true);

                await sf.Source(_session);
                await debugSession.NextPromptShouldBeBrowseAsync();
                await debugSession.ContinueAsync();
                await Task.Delay(100);

                var bp = await debugSession.CreateBreakpointAsync(sf, 3);

                await debugSession.NextPromptShouldBeBrowseAsync();
                (await debugSession.GetStackFramesAsync()).Should().HaveTail(new MatchDebugStackFrames {
                    { bp.Location }
                });
            }
        }

        [Test]
        [Category.R.Debugger]
        public async Task RemoveBreakpointWhileRunning() {
            const string code =
@"browser()
  f <- function() {
    NULL
    browser()
  }
  b <- FALSE;
  while (TRUE) if (b) f()";

            using (var debugSession = new DebugSession(_session))
            using (var sf = new SourceFile(code)) {
                await debugSession.EnableBreakpointsAsync(true);

                await sf.Source(_session);
                await debugSession.NextPromptShouldBeBrowseAsync();

                var bp = await debugSession.CreateBreakpointAsync(sf, 3);
                int hitCount = 0;
                bp.BreakpointHit += delegate { ++hitCount; };

                await debugSession.ContinueAsync();
                await Task.Delay(100);

                await bp.DeleteAsync();
                await _session.EvaluateAsync("b <- TRUE", REvaluationKind.Mutating);

                await debugSession.NextPromptShouldBeBrowseAsync();
                (await debugSession.GetStackFramesAsync()).Should().HaveTail(new MatchDebugStackFrames {
                    { sf, 4 }
                });
                hitCount.Should().Be(0);
            }
        }

        [Test]
        [Category.R.Debugger]
        public async Task BrowseOnNewPrompt() {
            using (var debugSession = new DebugSession(_session)) {
                var browseTask = EventTaskSources.DebugSession.Browse.Create(debugSession);

                using (var inter = await _session.BeginInteractionAsync()) {
                    await inter.RespondAsync("browser()\n");
                }

                await debugSession.NextPromptShouldBeBrowseAsync();
                await browseTask;
            }
        }

        [Test]
        [Category.R.Debugger]
        public async Task BrowseOnExistingPrompt() {
            using (var inter = await _session.BeginInteractionAsync()) {
                await inter.RespondAsync("browser()\n");
            }

            using (var inter = await _session.BeginInteractionAsync()) {
                inter.Contexts.IsBrowser().Should().BeTrue();
            }

            using (var debugSession = new DebugSession(_session)) {
                await debugSession.InitializeAsync();
                await EventTaskSources.DebugSession.Browse.Create(debugSession);
            }
        }
    }
}

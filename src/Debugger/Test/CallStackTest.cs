// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Debugger.Test {
    [ExcludeFromCodeCoverage]
    public class CallStackTest : IAsyncLifetime {
        private readonly MethodInfo _testMethod;
        private readonly RSessionProvider _sessionProvider;
        private readonly IRSession _session;

        public CallStackTest(TestMethodFixture testMethod) {
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
        public async Task CallStack() {
            using (var debugSession = new DebugSession(_session)) {
                const string code1 =
@"f <- function(n) {
     if (n > 0) {
        g(n - 1)
     } else {
        return()
     }
  }";

                const string code2 =
@"g <- function(n) {
     if (n > 0) {
        f(n - 1)
     } else {
        return()
     }
  }";

                using (var sf1 = new SourceFile(code1))
                using (var sf2 = new SourceFile(code2)) {
                    await debugSession.EnableBreakpointsAsync(true);

                    await sf1.Source(_session);
                    await sf2.Source(_session);

                    var bp = await debugSession.CreateBreakpointAsync(sf1, 5);
                    var bpHit = new BreakpointHitDetector(bp);

                    using (var inter = await _session.BeginInteractionAsync()) {
                        await inter.RespondAsync("f(4)\n");
                    }
                    await bpHit.ShouldBeHitAtNextPromptAsync();

                    var stackFrames = await debugSession.GetStackFramesAsync();
                    stackFrames.Should().HaveTail(new MatchDebugStackFrames {
                        { (string)null, null, "f(4)" },
                        { sf1, 3, "g(n - 1)" },
                        { sf2, 3, "f(n - 1)" },
                        { sf1, 3, "g(n - 1)" },
                        { sf2, 3, "f(n - 1)" },
                        { sf1, 5, MatchAny<string>.Instance },
                    });

                    stackFrames.First().EnvironmentName.Should().Be("<environment: R_GlobalEnv>");
                }
            }
        }

        [CompositeTest]
        [Category.R.Debugger]
        [InlineData(false)]
        [InlineData(true)]
        public async Task HideSourceFrames(bool debug) {
            using (var debugSession = new DebugSession(_session)) {
                using (var sf = new SourceFile("0")) { 
                    await debugSession.EnableBreakpointsAsync(true);

                    var bp = await debugSession.CreateBreakpointAsync(sf, 1);
                    var bpHit = new BreakpointHitDetector(bp);

                    await sf.Source(_session, debug);
                    await bpHit.ShouldBeHitAtNextPromptAsync();

                    var stackFrames = await debugSession.GetStackFramesAsync();
                    stackFrames.Should().HaveCount(1);
                    stackFrames[0].IsGlobal.Should().BeTrue();
                }
            }
        }
    }
}

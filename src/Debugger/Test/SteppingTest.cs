using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Debugger.Test {
    [ExcludeFromCodeCoverage]
    public class SteppingTest : IAsyncLifetime {
        private readonly MethodInfo _testMethod;
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRSession _session;

        private const string Code =
@"f <- function(x) {
  x + 1
}
x <- 1
y <- f(x)
z <- x + y";

        public SteppingTest(TestMethodInfoFixture testMethodInfo) {
            _testMethod = testMethodInfo.Method;
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
        public async Task StepOver() {
            
            using (var debugSession = new DebugSession(_session)) {
                using (var sf = new SourceFile(Code)) {
                    await debugSession.EnableBreakpointsAsync(true);

                    var bp = await debugSession.CreateBreakpointAsync(new DebugBreakpointLocation(sf.FilePath, 5));
                    var bpHit = new TaskCompletionSource<bool>();
                    bp.BreakpointHit += (s, e) => {
                        bpHit.TrySetResult(true);
                    };

                    await sf.Source(_session);
                    await bpHit.Task;

                    var stackFrames = (await debugSession.GetStackFramesAsync()).Reverse().ToArray();
                    stackFrames.Should().NotBeEmpty();
                    stackFrames[0].LineNumber.Should().Be(5);

                    bool stepCompleted = await debugSession.StepOverAsync();
                    stepCompleted.Should().Be(true);

                    stackFrames = (await debugSession.GetStackFramesAsync()).Reverse().ToArray();
                    stackFrames.Should().NotBeEmpty();
                    stackFrames[0].LineNumber.Should().Be(6);
                } 
            }
        }

        [Test]
        [Category.R.Debugger]
        public async Task StepInto() {
            using (var debugSession = new DebugSession(_session)) {
                using (var sf = new SourceFile(Code)) {
                    await debugSession.EnableBreakpointsAsync(true);

                    var bp = await debugSession.CreateBreakpointAsync(new DebugBreakpointLocation(sf.FilePath, 5));
                    var bpHit = new TaskCompletionSource<bool>();
                    bp.BreakpointHit += (s, e) => {
                        bpHit.TrySetResult(true);
                    };

                    await sf.Source(_session);
                    await bpHit.Task;

                    var stackFrames = (await debugSession.GetStackFramesAsync()).Reverse().ToArray();
                    stackFrames.Should().NotBeEmpty();
                    stackFrames[0].LineNumber.Should().Be(5);

                    bool stepCompleted = await debugSession.StepIntoAsync();
                    stepCompleted.Should().Be(true);

                    stackFrames = (await debugSession.GetStackFramesAsync()).Reverse().ToArray();
                    stackFrames.Should().HaveCount(n => n >= 2);
                    stackFrames[0].LineNumber.Should().Be(1);
                    stackFrames[1].Call.Should().Be("f(x)");
                }
                
            }
        }

        [Test(Skip = "https://github.com/Microsoft/RTVS/issues/975")]
        [Category.R.Debugger]
        public async Task StepOutToGlobal() {
            using (var debugSession = new DebugSession(_session)) {
                using (var sf = new SourceFile(Code)) {
                    await debugSession.EnableBreakpointsAsync(true);

                    var bp = await debugSession.CreateBreakpointAsync(new DebugBreakpointLocation(sf.FilePath, 2));
                    var bpHit = new TaskCompletionSource<bool>();
                    bp.BreakpointHit += (s, e) => {
                        bpHit.TrySetResult(true);
                    };

                    await sf.Source(_session);
                    await bpHit.Task;

                    var stackFrames = (await debugSession.GetStackFramesAsync()).Reverse().ToArray();
                    stackFrames.Should().HaveCount(n => n >= 2);
                    stackFrames[0].LineNumber.Should().Be(bp.Location.LineNumber);
                    stackFrames[1].Call.Should().Be("f(x)");

                    bool stepCompleted = await debugSession.StepOutAsync();
                    stepCompleted.Should().Be(true);

                    stackFrames = (await debugSession.GetStackFramesAsync()).Reverse().ToArray();
                    stackFrames.Should().HaveCount(n => n >= 2);
                    stackFrames[0].LineNumber.Should().Be(6);
                    stackFrames[1].Call.Should().NotBe("f(x)");
                }
                
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

            using (var debugSession = new DebugSession(_session)) {
                using (var sf = new SourceFile(code)) {
                    await debugSession.EnableBreakpointsAsync(true);

                    var bp = await debugSession.CreateBreakpointAsync(new DebugBreakpointLocation(sf.FilePath, 2));
                    var bpHit = new TaskCompletionSource<bool>();
                    bp.BreakpointHit += (s, e) => {
                        bpHit.SetResult(true);
                    };

                    await sf.Source(_session);
                    await bpHit.Task;

                    var stackFrames = (await debugSession.GetStackFramesAsync()).Reverse().ToArray();
                    stackFrames.Should().HaveCount(n => n >= 2);
                    stackFrames[0].LineNumber.Should().Be(bp.Location.LineNumber);
                    stackFrames[1].Call.Should().Be("f()");

                    bool stepCompleted = await debugSession.StepOutAsync();
                    stepCompleted.Should().Be(true);

                    stackFrames = (await debugSession.GetStackFramesAsync()).Reverse().ToArray();
                    stackFrames.Should().HaveCount(n => n >= 2);
                    stackFrames[0].LineNumber.Should().Be(6);
                    stackFrames[1].Call.Should().Be("g()");
                }
            }
        }

        [Test]
        [Category.R.Debugger]
        public async Task StepOutFromGlobal() {
            using (var debugSession = new DebugSession(_session)) {
                using (var sf = new SourceFile(Code)) {
                    await debugSession.EnableBreakpointsAsync(true);

                    var bp1 = await debugSession.CreateBreakpointAsync(new DebugBreakpointLocation(sf.FilePath, 4));
                    var bp2 = await debugSession.CreateBreakpointAsync(new DebugBreakpointLocation(sf.FilePath, 5));

                    var bpHit = new TaskCompletionSource<bool>();
                    bp1.BreakpointHit += (s, e) => {
                        bpHit.SetResult(true);
                    };

                    await sf.Source(_session);
                    await bpHit.Task;

                    var stackFrames = (await debugSession.GetStackFramesAsync()).Reverse().ToArray();
                    stackFrames[0].LineNumber.Should().Be(bp1.Location.LineNumber);

                    bool stepSuccessful = await debugSession.StepOutAsync();
                    stepSuccessful.Should().Be(false);

                    stackFrames = (await debugSession.GetStackFramesAsync()).Reverse().ToArray();
                    stackFrames[0].LineNumber.Should().Be(bp2.Location.LineNumber);
                }
            }
        }
    }
}

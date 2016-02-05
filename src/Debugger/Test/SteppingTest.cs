using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Test.Script;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Debugger.Test {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class SteppingTest {
        private const string code =
@"f <- function(x) {
  x + 1
}
x <- 1
y <- f(x)
z <- x + y";

        [Test]
        [Category.R.Debugger]
        public async Task StepOver() {
            var sessionProvider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            using (new RHostScript(sessionProvider)) {
                IRSession session = sessionProvider.GetOrCreate(GuidList.InteractiveWindowRSessionGuid, new RHostClientTestApp());
                using (var debugSession = new DebugSession(session)) {
                    using (var sf = new SourceFile(code)) {
                        await debugSession.EnableBreakpoints(true);

                        var bp = await debugSession.CreateBreakpointAsync(new DebugBreakpointLocation(sf.FilePath, 1));
                        var bpHit = new TaskCompletionSource<bool>();
                        bp.BreakpointHit += (s, e) => {
                            bpHit.SetResult(true);
                        };

                        await sf.Source(session);
                        await bpHit.Task;

                        var stackFrames = (await debugSession.GetStackFramesAsync()).Reverse().ToArray();
                        stackFrames.Should().NotBeEmpty();
                        stackFrames[0].LineNumber.Should().Be(1);

                        bool stepCompleted = await debugSession.StepOverAsync();
                        stepCompleted.Should().Be(true);

                        stackFrames = (await debugSession.GetStackFramesAsync()).Reverse().ToArray();
                        stackFrames.Should().NotBeEmpty();
                        stackFrames[0].LineNumber.Should().Be(2);
                    }
                }
            }
        }

        [Test]
        [Category.R.Debugger]
        public async Task StepInto() {
            var sessionProvider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            using (new RHostScript(sessionProvider)) {
                IRSession session = sessionProvider.GetOrCreate(GuidList.InteractiveWindowRSessionGuid, new RHostClientTestApp());
                using (var debugSession = new DebugSession(session)) {
                    using (var sf = new SourceFile(code)) {
                        await debugSession.EnableBreakpoints(true);

                        var bp = await debugSession.CreateBreakpointAsync(new DebugBreakpointLocation(sf.FilePath, 5));
                        var bpHit = new TaskCompletionSource<bool>();
                        bp.BreakpointHit += (s, e) => {
                            bpHit.SetResult(true);
                        };

                        await sf.Source(session);
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
        }

        [Test]
        [Category.R.Debugger]
        public async Task StepOut() {
            var sessionProvider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            using (new RHostScript(sessionProvider)) {
                IRSession session = sessionProvider.GetOrCreate(GuidList.InteractiveWindowRSessionGuid, new RHostClientTestApp());
                using (var debugSession = new DebugSession(session)) {
                    using (var sf = new SourceFile(code)) {
                        await debugSession.EnableBreakpoints(true);

                        var bp = await debugSession.CreateBreakpointAsync(new DebugBreakpointLocation(sf.FilePath, 2));
                        var bpHit = new TaskCompletionSource<bool>();
                        bp.BreakpointHit += (s, e) => {
                            bpHit.SetResult(true);
                        };

                        await sf.Source(session);
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
        }
    }
}

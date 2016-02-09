using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.R.Debugger.Test {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class ValuesTest {
        [Test]
        [Category.R.Debugger]
        public async Task MultilinePromise() {
            const string code = @"
f <- function(p, d) {
    force(d)
    browser()
}
x <- quote({{{}}})
eval(substitute(f(P, x), list(P = x)))
";

            var sessionProvider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            using (new RHostScript(sessionProvider)) {
                IRSession session = sessionProvider.GetOrCreate(GuidList.InteractiveWindowRSessionGuid, new RHostClientTestApp());
                using (var debugSession = new DebugSession(session)) {
                    using (var sf = new SourceFile(code)) {
                        await debugSession.EnableBreakpointsAsync(true);

                        var paused = new TaskCompletionSource<bool>();
                        debugSession.Browse += delegate {
                            paused.SetResult(true);
                        };

                        await sf.Source(session);
                        await paused.Task;

                        var stackFrames = (await debugSession.GetStackFramesAsync()).Reverse().ToArray();
                        stackFrames.Should().NotBeEmpty();

                        var evalResult = await stackFrames[0].GetEnvironmentAsync();
                        evalResult.Should().BeAssignableTo<DebugValueEvaluationResult>();

                        var frame = (DebugValueEvaluationResult)evalResult;
                        var children = (await frame.GetChildrenAsync()).ToDictionary(er => er.Name);

                        children.Should().ContainKey("p");
                        children["p"].Should().BeAssignableTo<DebugPromiseEvaluationResult>();
                        var p = (DebugPromiseEvaluationResult)children["p"];

                        children.Should().ContainKey("d");
                        children["d"].Should().BeAssignableTo<DebugValueEvaluationResult>();
                        var d = (DebugValueEvaluationResult)children["d"];

                        p.Code.Should().Be(d.Representation.Deparse);
                    }
                }
            }
        }
    }
}

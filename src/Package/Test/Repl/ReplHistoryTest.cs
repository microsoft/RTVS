using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.R.Support.Settings;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.R.Package.History;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.Shell.Mocks;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    public sealed class ReplHistoryTest {
        [Test]
        [Category.Repl]
        public async Task BasicHistoryTest() {
            var sessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            using (new RHostScript(sessionProvider)) {
                var historyProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRHistoryProvider>();
                var rInteractive = new RInteractive(sessionProvider, historyProvider, RToolsSettings.Current);
                var history = historyProvider.CreateRHistory(rInteractive);

                var session = sessionProvider.GetInteractiveWindowRSession();
                using (var eval = new RInteractiveEvaluator(session, history, RToolsSettings.Current)) {
                    var tb = new TextBufferMock(string.Empty, RContentTypeDefinition.ContentType);
                    var tv = new WpfTextViewMock(tb);

                    var iwm = new InteractiveWindowMock(tv);
                    eval.CurrentWindow = iwm;

                    var result = await eval.InitializeAsync();
                    result.Should().Be(ExecutionResult.Success);
                    session.IsHostRunning.Should().BeTrue();

                    result = await eval.ExecuteCodeAsync("x <- c(1:10)");
                    result.Should().Be(ExecutionResult.Success);
                    history.HasEntries.Should().BeTrue();
                    history.HasSelectedEntries.Should().BeFalse();
                    history.SelectHistoryEntry(0);
                    history.HasSelectedEntries.Should().BeTrue();

                    tb.Clear();
                    history.SendSelectedToTextView(tv);
                    text = tb.CurrentSnapshot.GetText();
                    text.Should().Be("x <- c(1:10)");
                }
            }
        }
    }
}

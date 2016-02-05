using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.R.Package.History;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Test.Utility;
using Microsoft.VisualStudio.Shell.Mocks;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public sealed class RInteractiveEvaluatorTest {
        [Test]
        [Category.Repl]
        public async Task EvaluatorTest() {
            using (new VsRHostScript()) {
                var sessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
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

                    eval.CanExecuteCode("x <-").Should().BeFalse();
                    eval.CanExecuteCode("(()").Should().BeFalse();
                    eval.CanExecuteCode("a *(b+c)").Should().BeTrue();

                    result = await eval.ExecuteCodeAsync(new string(new char[10000]));
                    result.Should().Be(ExecutionResult.Failure);
                    string text = tb.CurrentSnapshot.GetText();
                    text.Should().Contain(string.Format(Resources.InputIsTooLong, 4096));

                    tb.Clear();

                    result = await eval.ExecuteCodeAsync("電話帳 全米のお");
                    result.Should().Be(ExecutionResult.Failure);
                    text = tb.CurrentSnapshot.GetText();
                    text.Should().Be(Resources.Error_ReplUnicodeCoversion);

                    tb.Clear();

                    result = await eval.ExecuteCodeAsync("x <- c(1:10)");
                    result.Should().Be(ExecutionResult.Success);
                    text = tb.CurrentSnapshot.GetText();
                    text.Should().Be(string.Empty);

                    tb.Clear();

                    await eval.ResetAsync(initialize: false);
                    text = tb.CurrentSnapshot.GetText();
                    text.Should().StartWith(Resources.MicrosoftRHostStopping);
                }
            }
        }
    }
}

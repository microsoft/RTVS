using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Actions.Utility;
using Microsoft.R.Support.Settings;
using Microsoft.VisualStudio.InteractiveWindow;

namespace Microsoft.VisualStudio.R.Package.Repl {
    internal sealed class NullInteractiveEvaluator : IInteractiveEvaluator {
        public void Dispose() { }

        public Task<ExecutionResult> InitializeAsync() => ExecutionResult.Failed;

        public Task<ExecutionResult> ResetAsync(bool initialize = true) {
            if (initialize) {
                if (RInstallation.VerifyRIsInstalled(RToolsSettings.Current.RBasePath)) {
                    EditorShell.Current.ShowErrorMessage(Resources.Error_RestartVsAfterRInstalled);
                }
                else {
                    EditorShell.Current.ShowErrorMessage(Resources.Error_RNotInstalled);
                    Process.Start("https://cran.r-project.org");
                }
            }

            return ExecutionResult.Failed;
        }

        public bool CanExecuteCode(string text) => false;

        public Task<ExecutionResult> ExecuteCodeAsync(string text) => ExecutionResult.Failed;

        public string FormatClipboard() => null;

        public void AbortExecution() { }

        public string GetPrompt() => string.Empty;

        public IInteractiveWindow CurrentWindow { get; set; }
    }
}
using System.Threading.Tasks;
using Microsoft.VisualStudio.InteractiveWindow;

namespace Microsoft.VisualStudio.Shell.Mocks {
    using Task = System.Threading.Tasks.Task;

    public sealed class InteractiveEvaluatorMock : IInteractiveEvaluator {
        public InteractiveEvaluatorMock(IInteractiveWindow window) {
            CurrentWindow = window;
        }
        public IInteractiveWindow CurrentWindow { get; set; }

        public void AbortExecution() {
        }

        public bool CanExecuteCode(string text) {
            return true;
        }

        public void Dispose() {
        }

        public Task<ExecutionResult> ExecuteCodeAsync(string text) {
            return Task.FromResult(ExecutionResult.Success);
        }

        public string FormatClipboard() {
            return string.Empty;
        }

        public string GetPrompt() {
            return ">";
        }

        public Task<ExecutionResult> InitializeAsync() {
            return Task.FromResult(ExecutionResult.Success);
        }

        public Task<ExecutionResult> ResetAsync(bool initialize = true) {
            return Task.FromResult(ExecutionResult.Success);
        }
    }
}

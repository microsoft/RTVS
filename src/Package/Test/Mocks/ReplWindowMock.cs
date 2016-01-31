using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.InteractiveWindow.Shell;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Mocks;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.Test.Mocks {
    public sealed class ReplWindowMock : IReplWindow {
        public bool IsActive { get; set; }
        public string EnqueuedCode { get; set; }
        public string ExecutedCode { get; set; }

        public void ClearPendingInputs() {
        }

        public void Dispose() {
        }

        public void EnqueueCode(string code, bool addNewLine) {
            EnqueuedCode = code;
        }

        public void ExecuteCode(string code) {
            ExecutedCode = code;
        }

        public void ExecuteCurrentExpression(ITextView textView) {
        }

        public IVsInteractiveWindow GetInteractiveWindow() {
            return new VsInteractiveWindowMock(
                        new WpfTextViewMock(
                            new TextBufferMock(string.Empty, RContentTypeDefinition.ContentType)));
        }

        public void Show() {
            var frame = ReplWindow.FindReplWindowFrame(__VSFINDTOOLWIN.FTW_fFindFirst);
            frame.Show();
        }

        public void ReplaceCurrentExpression(string replaceWith) {
            ExecutedCode = replaceWith;
            EnqueuedCode = replaceWith;
        }
    }
}

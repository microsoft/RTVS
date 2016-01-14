using System;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Completion {
    internal sealed class AsyncCompletionSession: IDisposable {
        private const string _asyncIntellisenseSession = "Async R Completion Session";
        private ICompletionSession _asyncSession;

        public AsyncCompletionSession(ICompletionSession session) {
            _asyncSession = session;
            _asyncSession.Properties.AddProperty(_asyncIntellisenseSession, String.Empty);
            _asyncSession.TextView.TextBuffer.Changed += OnTextBufferChanged;
        }

        public event EventHandler Dismissed;

        public void Dispose() {
            if (_asyncSession != null && _asyncSession.TextView != null && _asyncSession.TextView.TextBuffer != null) {
                _asyncSession.TextView.TextBuffer.Changed -= OnTextBufferChanged;
            }
            _asyncSession = null;
        }

        private void OnTextBufferChanged(object sender, TextContentChangedEventArgs e) {
            Dismiss();
            Dispose();
        }

        private void Dismiss() {
            if (_asyncSession != null && _asyncSession.Properties != null && _asyncSession.Properties.ContainsProperty(_asyncIntellisenseSession) && !_asyncSession.IsDismissed) {
                var controller = RCompletionController.FromTextView(_asyncSession.TextView);
                if (controller != null) {
                    controller.DismissCompletionSession();
                }
            }

            if(Dismissed != null) {
                Dismissed(this, EventArgs.Empty);
            }

            _asyncSession = null;
        }

        public void Complete() {
            if (_asyncSession == null) {
                return;
            }

            RCompletionController controller = RCompletionController.FromTextView(_asyncSession.TextView);
            _asyncSession = null;
            if (controller != null) {
                controller.ShowCompletion(autoShownCompletion: true);
                controller.FilterCompletionSession();
            }
        }
    }
}

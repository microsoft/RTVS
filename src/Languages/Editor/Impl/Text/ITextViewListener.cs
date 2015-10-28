using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.Text {
    public interface ITextViewListener {
        /// <summary>
        /// Called when a text buffer gets attached to its first view
        /// </summary>
        void OnTextViewConnected(ITextView textView, ITextBuffer textBuffer);

        /// <summary>
        /// Called when a text buffer is detached from its last view
        /// </summary>
        void OnTextViewDisconnected(ITextView textView, ITextBuffer textBuffer);
    }
}

using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.History {
    public interface IRHistoryProvider {
        IRHistory GetAssociatedRHistory(ITextView textView);
    }
}
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.History {
    public interface IRHistoryProvider {
        IRHistory CreateRHistory(IRInteractiveSession interactiveSession);
        IRHistory GetAssociatedRHistory(ITextView textView);
        IRHistoryFiltering CreateFiltering(IRHistory history);
        IWpfTextView GetOrCreateTextView(IRHistory history);
    }
}
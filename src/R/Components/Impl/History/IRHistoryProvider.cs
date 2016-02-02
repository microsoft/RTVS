using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Components.History {
    public interface IRHistoryProvider {
        IRHistory CreateRHistory(IRInteractiveWorkflow interactiveWorkflow);
        IRHistory GetAssociatedRHistory(ITextView textView);
        IRHistoryFiltering CreateFiltering(IRHistory history);
        IWpfTextView GetOrCreateTextView(IRHistory history);
    }
}
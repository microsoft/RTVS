using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Components.History {
    public interface IRHistoryEntry {
        ITrackingSpan EntrySpan { get; }
        ITrackingSpan Span { get; }
        bool IsSelected { get; set; }

        IRHistoryEntry Next { get; }
        IRHistoryEntry Previous { get; }
    }
}
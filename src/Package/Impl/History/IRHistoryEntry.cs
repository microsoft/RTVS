using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.R.Package.History {
    public interface IRHistoryEntry {
        string Text { get; }
        ITrackingSpan TrackingSpan { get; set; }
        bool IsSelected { get; set; }

        IRHistoryEntry Next { get; }
        IRHistoryEntry Previous { get; }
    }
}
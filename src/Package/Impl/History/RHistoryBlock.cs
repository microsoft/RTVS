using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.R.Package.History {
    internal sealed class RHistoryBlock {
        public RHistoryBlock(string text) {
            Text = text;
        }

        public string Text { get; }
        public ITrackingSpan TrackingSpan { get; set; }
        public bool IsSelected { get; set; }
    }
}
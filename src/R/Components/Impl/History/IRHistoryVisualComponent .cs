using Microsoft.R.Components.View;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Components.History {
    public interface IRHistoryWindowVisualComponent : IVisualComponent {
        IWpfTextView TextView { get; }
    }
}

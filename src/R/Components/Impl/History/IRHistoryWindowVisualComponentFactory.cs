using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Components.History {
    public interface IRHistoryWindowVisualComponentFactory {
        IRHistoryWindowVisualComponent Create(ITextBuffer historyTextBuffer, int instanceId = 0);
    }
}
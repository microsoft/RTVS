using Microsoft.R.Components.View;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Components.History {
    public interface IRHistoryVisualComponentContainerFactory {
        IVisualComponentContainer<IRHistoryWindowVisualComponent> GetOrCreate(ITextBuffer historyTextBuffer, int instanceId = 0);
    }
}
using System;
using System.Threading.Tasks;
using Microsoft.R.Components.History;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Components.InteractiveWorkflow {
    public interface IRInteractiveWorkflow : IDisposable {
        IRHistory History { get; }
        IRSession RSession { get; }
        IRInteractiveWorkflowOperations Operations { get; }
        IInteractiveWindowVisualComponent ActiveWindow { get; }

        Task<IInteractiveWindowVisualComponent> CreateInteractiveWindowAsync(IInteractiveWindowComponentFactory interactiveWindowComponentFactory, int instanceId = 0);
    }
}
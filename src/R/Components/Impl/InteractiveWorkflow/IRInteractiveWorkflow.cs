using System;
using System.Threading.Tasks;
using Microsoft.R.Components.History;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Components.InteractiveWorkflow {
    public interface IRInteractiveWorkflow : IDisposable {
        IRHistory History { get; }
        IRSession RSession { get; }
        IRInteractiveWorkflowOperations Operations { get; }
        IInteractiveWindowVisualComponent ActiveWindow { get; }

        Task<IInteractiveWindowVisualComponent> CreateInteractiveWindowAsync(IInteractiveWindowComponentFactory interactiveWindowComponentFactory, IContentTypeRegistryService contentTypeRegistryService, int instanceId = 0);
    }
}
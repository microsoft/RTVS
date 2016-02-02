using System.Threading.Tasks;

namespace Microsoft.R.Components.InteractiveWorkflow {
    public interface IRInteractiveWorkflowProvider {
        IRInteractiveWorkflow GetOrCreate();
        Task<IInteractiveWindowVisualComponent> CreateInteractiveWindowAsync(IRInteractiveWorkflow workflow, int instanceId = 0);
    }
}
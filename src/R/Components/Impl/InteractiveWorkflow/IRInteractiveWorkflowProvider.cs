using System.Threading.Tasks;

namespace Microsoft.R.Components.InteractiveWorkflow {
    public interface IRInteractiveWorkflowProvider {
        IRInteractiveWorkflow GetOrCreate();
    }
}
using System.Threading.Tasks;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Test.Stubs {
    public class RInteractiveWorkflowProviderStub : IRInteractiveWorkflowProvider {
        private readonly IRInteractiveWorkflow _workflow;
        private readonly IInteractiveWindowComponentFactory _componentFactory;

        public RInteractiveWorkflowProviderStub(IRInteractiveWorkflow workflow, IInteractiveWindowComponentFactory componentFactory) {
            _workflow = workflow;
            _componentFactory = componentFactory;
        }

        public IRInteractiveWorkflow GetOrCreate() {
            return _workflow;
        }

        public Task<IInteractiveWindowVisualComponent> CreateInteractiveWindowAsync(IRInteractiveWorkflow workflow, int instanceId = 0) {
            return workflow.CreateInteractiveWindowAsync(_componentFactory, instanceId);
        }
    }
}

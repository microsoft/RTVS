using System.Threading.Tasks;
using Microsoft.R.Components.InteractiveWorkflow;

namespace Microsoft.R.Components.Test.Stubs {
    public class RInteractiveWorkflowProviderStub : IRInteractiveWorkflowProvider {
        private readonly IRInteractiveWorkflow _workflow;
        private readonly IInteractiveWindowComponentContainerFactory _componentContainerFactory;

        public RInteractiveWorkflowProviderStub(IRInteractiveWorkflow workflow, IInteractiveWindowComponentContainerFactory componentContainerFactory) {
            _workflow = workflow;
            _componentContainerFactory = componentContainerFactory;
        }

        public IRInteractiveWorkflow GetOrCreate() {
            return _workflow;
        }

        public Task<IInteractiveWindowVisualComponent> CreateInteractiveWindowAsync(IRInteractiveWorkflow workflow, int instanceId = 0) {
            return workflow.GetOrCreateVisualComponent(_componentContainerFactory, instanceId);
        }
    }
}

using Microsoft.VisualStudio.InteractiveWindow;

namespace Microsoft.R.Components.InteractiveWorkflow {
    public interface IInteractiveWindowComponentContainerFactory {
        IInteractiveWindowVisualComponent Create(int instanceId, IInteractiveEvaluator evaluator);
    }
}
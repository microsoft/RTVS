using Microsoft.VisualStudio.InteractiveWindow;

namespace Microsoft.R.Components.InteractiveWorkflow {
    public interface IInteractiveWindowComponentFactory {
        IInteractiveWindowVisualComponent Create(int instanceId, IInteractiveEvaluator evaluator);
    }
}
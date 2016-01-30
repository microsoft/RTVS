using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.InteractiveWindow;
using Microsoft.VisualStudio.R.Package.History;

namespace Microsoft.VisualStudio.R.Package.Repl {
    public interface IRInteractive {
        IRHistory History { get; }
        IRSession RSession { get; }
        IInteractiveEvaluator GetOrCreateEvaluator(int instanceId);
    }
}
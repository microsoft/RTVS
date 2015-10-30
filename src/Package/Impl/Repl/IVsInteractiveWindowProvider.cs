using Microsoft.VisualStudio.InteractiveWindow.Shell;

namespace Microsoft.VisualStudio.R.Package.Repl {
    public interface IVsInteractiveWindowProvider {
        IVsInteractiveWindow Create(int instanceId);
        void Open(int instanceId, bool focus);
    }
}
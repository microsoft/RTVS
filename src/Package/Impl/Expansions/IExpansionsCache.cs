using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Expansions {
    public interface IExpansionsCache {
        VsExpansion? GetExpansion(string shortcut);
    }
}

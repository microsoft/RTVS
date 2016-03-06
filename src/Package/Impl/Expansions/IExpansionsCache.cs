using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Expansions {
    internal interface IExpansionsCache {
        VsExpansion? GetExpansion(string shortcut);
    }
}

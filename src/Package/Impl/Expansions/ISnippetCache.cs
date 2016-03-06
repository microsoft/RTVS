using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Expansions {
    internal interface ISnippetCache {
        VsExpansion? GetExpansion(string shortcut);
    }
}

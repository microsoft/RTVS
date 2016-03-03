using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Snippets.Definitions {
    internal interface ISnippetCache {
        VsExpansion? GetExpansion(string shortcut);
    }
}

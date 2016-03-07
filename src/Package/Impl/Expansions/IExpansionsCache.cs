using Microsoft.R.Editor.Snippets;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.R.Package.Expansions {
    public interface IExpansionsCache: ISnippetInformationSource {
        VsExpansion? GetExpansion(string shortcut);
    }
}

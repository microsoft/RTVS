using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using static System.FormattableString;

namespace Microsoft.R.Editor.SuggestedActions.Actions {
    internal sealed class InstallPackageSuggestedAction : LibrarySuggestedAction {
        public InstallPackageSuggestedAction(ITextView textView, ITextBuffer textBuffer, int position) :
            base(textView, textBuffer, position, Resources.SmartTagName_InstallPackage) { }

        protected override string GetCommand(string libraryName) {
            return Invariant($"install.packages('{libraryName}')");
        }
    }
}

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using static System.FormattableString;

namespace Microsoft.R.Editor.SuggestedActions.Actions {
    internal sealed class LoadLibrarySuggestedAction : LibrarySuggestedAction {

        public LoadLibrarySuggestedAction(ITextView textView, ITextBuffer textBuffer, int position) :
            base(textView, textBuffer, position, Resources.SmartTagName_LoadLibrary) { }

        protected override string GetCommand(string libraryName) {
            return Invariant($"library({libraryName})");
        }
    }
}

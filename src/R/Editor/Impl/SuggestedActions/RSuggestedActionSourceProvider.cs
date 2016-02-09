using System.ComponentModel.Composition;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.SuggestedActions {
    [Export(typeof(ISuggestedActionsSourceProvider))]
    [Name("RSuggestedActionSourceProvider")]
    [ContentType(RContentTypeDefinition.ContentType)]
    internal sealed class RSuggestedActionSourceProvider : ISuggestedActionsSourceProvider {
        public ISuggestedActionsSource CreateSuggestedActionsSource(ITextView textView, ITextBuffer textBuffer) {
            return RSuggestedActionSource.FromViewAndBuffer(textView, textBuffer);
        }
    }
}

using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.BraceMatch.Definitions;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.BraceMatch {
    [Export(typeof(IBraceMatcherProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    internal class RBraceMatcherProvider : IBraceMatcherProvider {
        public IBraceMatcher CreateBraceMatcher(ITextView textView, ITextBuffer textBuffer) {
            return new RBraceMatcher(textView, textBuffer);
        }
    }
}

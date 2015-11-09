using Microsoft.VisualStudio.Text.Tagging;

namespace Microsoft.Languages.Editor.BraceMatch {
    public class BraceHighlightTag : TextMarkerTag {
        public BraceHighlightTag()
            : base("MarkerFormatDefinition/HighlightedReference") {
        }
    }
}
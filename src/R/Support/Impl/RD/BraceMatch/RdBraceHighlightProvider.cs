using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.BraceMatch;
using Microsoft.R.Support.RD.ContentTypes;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Support.RD.BraceMatch {
    [Export(typeof(IViewTaggerProvider))]
    [ContentType(RdContentTypeDefinition.ContentType)]
    [TagType(typeof(TextMarkerTag))]
    internal sealed class RdBraceHighlightProvider : BraceHighlightProvider
    {
    }
}

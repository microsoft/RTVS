using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.BraceMatch;
using Microsoft.R.Editor.ContentType;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.BraceMatch
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [TagType(typeof(TextMarkerTag))]
    internal sealed class RBraceHighlightProvider : BraceHighlightProvider
    {
    }
}

using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.ContentType;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Validation.Tagger
{
    [Export(typeof(ITaggerProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [TagType(typeof(ErrorTag))]
    internal sealed class EditorErrorTaggerProvider : ITaggerProvider
    {
        public ITagger<T> CreateTagger<T>(ITextBuffer textBuffer) where T : ITag
        {
            EditorErrorTagger tagger = null;

            if (EditorDocument.FromTextBuffer(textBuffer) != null)
            {
                tagger = ServiceManager.GetService<EditorErrorTagger>(textBuffer);
                if (tagger == null)
                    tagger = new EditorErrorTagger(textBuffer);
            }

            return tagger as ITagger<T>;
        }
    }
}

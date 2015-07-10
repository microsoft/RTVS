using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.ContentType;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Editor.Document;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Outline
{
    [Export(typeof(ITaggerProvider))]
    [TagType(typeof(IOutliningRegionTag))]
    [ContentType(RContentTypeDefinition.ContentType)]
    internal sealed class OutliningTaggerProvider : ITaggerProvider
    {
        #region ITaggerProvider
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            var tagger = ServiceManager.GetService<ROutliningTagger>(buffer);
            if (tagger == null)
            {
                var document = ServiceManager.GetService<EditorDocument>(buffer);
                if (document != null)
                    tagger = new ROutliningTagger(document);
            }
            return tagger as ITagger<T>;
        }
        #endregion
    }
}

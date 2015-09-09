using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Support.Markdown.ContentTypes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Support.Markdown.Classification
{
    [Export(typeof(IClassifierProvider))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    internal sealed class ClassifierProvider : IClassifierProvider
    {
        [Import]
        public IClassificationTypeRegistryService ClassificationRegistryService { get; set; }

        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            MdClassifier classifier = ServiceManager.GetService<MdClassifier>(textBuffer);
            if (classifier == null)
            {
                classifier = new MdClassifier(textBuffer, ClassificationRegistryService);
            }

            return classifier;
        }
    }
}

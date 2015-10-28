using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Support.RD.ContentTypes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Support.RD.Classification {
    [Export(typeof(IClassifierProvider))]
    [ContentType(RdContentTypeDefinition.ContentType)]
    internal sealed class ClassifierProvider : IClassifierProvider {
        [Import]
        public IClassificationTypeRegistryService ClassificationRegistryService { get; set; }

        public IClassifier GetClassifier(ITextBuffer textBuffer) {
            RdClassifier classifier = ServiceManager.GetService<RdClassifier>(textBuffer);
            if (classifier == null) {
                classifier = new RdClassifier(textBuffer, ClassificationRegistryService);
            }

            return classifier;
        }
    }
}

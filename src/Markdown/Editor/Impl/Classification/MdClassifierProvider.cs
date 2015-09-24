using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Languages.Core.Classification;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Services;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Classification
{
    [Export(typeof(IClassifierProvider))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    internal sealed class MdClassifierProvider : IClassifierProvider
    {
        [Import]
        public IClassificationTypeRegistryService ClassificationRegistryService { get; set; }

        [Import]
        private IContentTypeRegistryService ContentTypeRegistryService { get; set; }

        [ImportMany]
        private IEnumerable<Lazy<IClassificationNameProvider, IComponentContentTypes>> ClassificationNameProviders { get; set; }

        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            MdClassifier classifier = ServiceManager.GetService<MdClassifier>(textBuffer);
            if (classifier == null)
            {
                classifier = new MdClassifier(textBuffer, ClassificationRegistryService, ContentTypeRegistryService, ClassificationNameProviders);
            }

            return classifier;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Languages.Core.Classification;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Classification.MD
{
    [Export(typeof(IClassifierProvider))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    internal sealed class MdClassifierProvider : MarkdownClassifierProvider<MdClassifierProvider>
    {
        protected override IClassifier CreateClassifier(ITextBuffer textBuffer, IClassificationTypeRegistryService crs, IContentTypeRegistryService ctrs, IEnumerable<Lazy<IClassificationNameProvider, IComponentContentTypes>> ClassificationNameProviders)
        {
            return new MdClassifier(textBuffer, ClassificationRegistryService, ContentTypeRegistryService, ClassificationNameProviders);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Languages.Core.Classification;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Classification.RMD
{
    [Export(typeof(IClassifierProvider))]
    [ContentType(RmdContentTypeDefinition.ContentType)]
    internal sealed class RmdClassifierProvider : MarkdownClassifierProvider<RmdClassifierProvider>
    {
        protected override IClassifier CreateClassifier(ITextBuffer textBuffer, IClassificationTypeRegistryService crs, IContentTypeRegistryService ctrs, IEnumerable<Lazy<IClassificationNameProvider, IComponentContentTypes>> ClassificationNameProviders)
        {
            return new RmdClassifier(textBuffer, ClassificationRegistryService, ContentTypeRegistryService, ClassificationNameProviders);
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Classification.MD {
    [Export(typeof(IClassifierProvider))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    internal sealed class MdClassifierProvider : MarkdownClassifierProvider<MdClassifierProvider> {
        private readonly IClassificationTypeRegistryService _classificationRegistryService;
        private readonly IContentTypeRegistryService _contentTypeRegistryService;

        [ImportingConstructor]
        public MdClassifierProvider(IClassificationTypeRegistryService crs, IContentTypeRegistryService ctrs) {
            _classificationRegistryService = crs;
            _contentTypeRegistryService = ctrs;
        }

        protected override IClassifier CreateClassifier(ITextBuffer textBuffer)
            => textBuffer.Properties.GetOrCreateSingletonProperty(() => new MdClassifier(textBuffer, _classificationRegistryService, _contentTypeRegistryService));
    }
}

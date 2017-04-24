// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Text;
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
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public MdClassifierProvider(IClassificationTypeRegistryService crs, IContentTypeRegistryService ctrs, ICoreShell shell) {
            _classificationRegistryService = crs;
            _contentTypeRegistryService = ctrs;
            _shell = shell;
        }

        protected override IClassifier CreateClassifier(ITextBuffer textBuffer) {
            var classifier = textBuffer.GetService<MdClassifier>();
            return classifier ?? new MdClassifier(textBuffer, _classificationRegistryService, _contentTypeRegistryService);
        }
    }
}

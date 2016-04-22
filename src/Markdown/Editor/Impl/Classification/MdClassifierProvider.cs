// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Languages.Core.Classification;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Classification.MD {
    [Export(typeof(IClassifierProvider))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    internal sealed class MdClassifierProvider : MarkdownClassifierProvider<MdClassifierProvider> {
        [Import]
        private IClassificationTypeRegistryService ClassificationRegistryService { get; set; }

        [Import]
        private IContentTypeRegistryService ContentTypeRegistryService { get; set; }

        [ImportMany]
        private IEnumerable<Lazy<IClassificationNameProvider, IComponentContentTypes>> ClassificationNameProviders { get; set; }

        protected override IClassifier CreateClassifier(ITextBuffer textBuffer) {
            return new MdClassifier(textBuffer, ClassificationRegistryService, ContentTypeRegistryService, ClassificationNameProviders);
        }
    }
}

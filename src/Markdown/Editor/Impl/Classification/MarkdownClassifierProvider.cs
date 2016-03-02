// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Languages.Core.Classification;
using Microsoft.Languages.Editor.Composition;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.Classification {
    internal abstract class MarkdownClassifierProvider<T> : IClassifierProvider where T : class {
        [Import]
        protected IClassificationTypeRegistryService ClassificationRegistryService { get; set; }

        [Import]
        protected IContentTypeRegistryService ContentTypeRegistryService { get; set; }

        [ImportMany]
        protected IEnumerable<Lazy<IClassificationNameProvider, IComponentContentTypes>> ClassificationNameProviders { get; set; }

        protected MarkdownClassifierProvider() {
            EditorShell.Current.CompositionService.SatisfyImportsOnce(this);
        }

        public IClassifier GetClassifier(ITextBuffer textBuffer) {
            IClassifier classifier = ServiceManager.GetService<T>(textBuffer) as IClassifier;
            if (classifier == null) {
                classifier = CreateClassifier(textBuffer, ClassificationRegistryService, ContentTypeRegistryService, ClassificationNameProviders);
            }

            return classifier;
        }

        protected abstract IClassifier CreateClassifier(ITextBuffer textBuffer,
                                               IClassificationTypeRegistryService crs,
                                               IContentTypeRegistryService ctrs,
                                               IEnumerable<Lazy<IClassificationNameProvider, IComponentContentTypes>> ClassificationNameProviders);
    }
}

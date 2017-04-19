// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Services;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.Markdown.Editor.Classification {
    internal abstract class MarkdownClassifierProvider<T> : IClassifierProvider where T : class {
        public IClassifier GetClassifier(ITextBuffer textBuffer) {
            IClassifier classifier = ServiceManager.GetService<T>(textBuffer) as IClassifier;
            if (classifier == null) {
                classifier = CreateClassifier(textBuffer);
            }
            return classifier;
        }

        protected abstract IClassifier CreateClassifier(ITextBuffer textBuffer);
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Editor.Text;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Microsoft.Markdown.Editor.Classification {
    internal abstract class MarkdownClassifierProvider<T> : IClassifierProvider where T : class {
        public IClassifier GetClassifier(ITextBuffer textBuffer) {
            var classifier = textBuffer.GetService<T>() as IClassifier;
            return classifier ?? CreateClassifier(textBuffer);
        }

        protected abstract IClassifier CreateClassifier(ITextBuffer textBuffer);
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Editor.RData.ContentTypes;
using Microsoft.R.Support.RD.ContentTypes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.RData.Classification {
    [Export(typeof(IClassifierProvider))]
    [ContentType(RdContentTypeDefinition.ContentType)]
    internal sealed class ClassifierProvider : IClassifierProvider {
        [Import]
        public IClassificationTypeRegistryService ClassificationRegistryService { get; set; }

        public IClassifier GetClassifier(ITextBuffer textBuffer) {
            var classifier = textBuffer.GetService<RdClassifier>();
            return classifier ?? new RdClassifier(textBuffer, ClassificationRegistryService);
        }
    }
}

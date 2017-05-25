// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.R.Editor.RData.ContentTypes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.RData.Classification {
    [Export(typeof(IClassifierProvider))]
    [ContentType(RdContentTypeDefinition.ContentType)]
    internal sealed class ClassifierProvider : IClassifierProvider {
        private readonly IClassificationTypeRegistryService _ctrs;

        public ClassifierProvider(IClassificationTypeRegistryService ctrs) {
            _ctrs = ctrs;
        }

        public IClassifier GetClassifier(ITextBuffer textBuffer)
            => textBuffer.Properties.GetOrCreateSingletonProperty(() => new RdClassifier(textBuffer, _ctrs));
    }
}

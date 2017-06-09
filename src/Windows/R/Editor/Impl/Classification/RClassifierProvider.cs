// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.History;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Classification {
    [Export(typeof(IClassifierProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [ContentType(RHistoryContentTypeDefinition.ContentType)]
    internal sealed class RClassifierProvider : IClassifierProvider {
        private readonly IClassificationTypeRegistryService _crs;

        [ImportingConstructor]
        public RClassifierProvider(IClassificationTypeRegistryService crs) {
            _crs = crs;
        }

        // Classifier can be fetched before document is created
        // so we do not use service manager here.
        public IClassifier GetClassifier(ITextBuffer textBuffer)
            => textBuffer.Properties.GetOrCreateSingletonProperty(() => new RClassifier(textBuffer, _crs));

        public static RClassifier GetRClassifier(ITextBuffer textBuffer) => RClassifier.FromTextBuffer(textBuffer);
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.History;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Classification {
    [Export(typeof(IClassifierProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [ContentType(RHistoryContentTypeDefinition.ContentType)]
    internal sealed class ClassifierProvider : IClassifierProvider {
        private readonly IClassificationTypeRegistryService _crs;

        [ImportingConstructor]
        public ClassifierProvider(IClassificationTypeRegistryService crs) {
            _crs = crs;
        }

        public IClassifier GetClassifier(ITextBuffer textBuffer) {
            var classifier = textBuffer.GetService<RClassifier>();
            return classifier ?? new RClassifier(textBuffer, _crs);
        }
    }
}

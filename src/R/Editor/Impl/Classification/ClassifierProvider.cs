// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Services;
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
        [Import]
        public IClassificationTypeRegistryService ClassificationRegistryService { get; set; }

        [Import]
        public ICoreShell Shell { get; set; }

        public IClassifier GetClassifier(ITextBuffer textBuffer) {
            RClassifier classifier = ServiceManager.GetService<RClassifier>(textBuffer);
            if (classifier == null) {
                classifier = new RClassifier(textBuffer, ClassificationRegistryService);
                ServiceManager.AddService<RClassifier>(classifier, textBuffer, Shell);
            }

            return classifier;
        }
    }
}

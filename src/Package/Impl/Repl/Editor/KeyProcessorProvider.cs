// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.R.Package.Repl.Editor {
    [Export(typeof(IKeyProcessorProvider))]
    [Name(nameof(ReplKeyProcessor))]
    [Order(Before = "Default")]
    [ContentType("Interactive Content")]
    [ContentType(RContentTypeDefinition.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class KeyProcessorProvider : IKeyProcessorProvider {
        public KeyProcessor GetAssociatedProcessor(IWpfTextView wpfTextView) {
            return wpfTextView.Properties.GetOrCreateSingletonProperty(() => new ReplKeyProcessor(wpfTextView));
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Commands {
    [Export(typeof(IMouseProcessorProvider))]
    [Name(nameof(RMouseProcessor))]
    [Order(Before = "WordSelection")]
    [ContentType(RContentTypeDefinition.ContentType)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class RMouseProcessorProvider : IMouseProcessorProvider {
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public RMouseProcessorProvider(ICoreShell shell) {
            _shell = shell;
        }

        public IMouseProcessor GetAssociatedProcessor(IWpfTextView wpfTextView) 
            => wpfTextView.Properties.GetOrCreateSingletonProperty(() => new RMouseProcessor(wpfTextView, _shell));
    }
}

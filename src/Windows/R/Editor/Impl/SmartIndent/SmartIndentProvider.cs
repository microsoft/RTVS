// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.SmartIndent {
    [Export(typeof(ISmartIndentProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [Name("R Smart Indent")]
    internal class SmartIndentProvider : ISmartIndentProvider {
        private readonly IREditorSettings _settings;

        [ImportingConstructor]
        public SmartIndentProvider(ICoreShell coreShell) {
            _settings = coreShell.GetService<IREditorSettings>();
        }

        public ISmartIndent CreateSmartIndent(ITextView textView)
            => textView.Properties.GetOrCreateSingletonProperty(() => new SmartIndent(textView, _settings));
    }
}

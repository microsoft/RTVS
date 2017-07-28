// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.SmartIndent {
    [Export(typeof(ISmartIndentProvider))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    [Name("R Markdown Smart Indent")]
    internal class SmartIndentProvider : ISmartIndentProvider {
        private readonly ICoreShell _coreShell;

        [ImportingConstructor]
        public SmartIndentProvider(ICoreShell coreShell) {
            _coreShell = coreShell;
        }

       public ISmartIndent CreateSmartIndent(ITextView textView)
            => textView.Properties.GetOrCreateSingletonProperty(() => new SmartIndent(textView, _coreShell.Services));
    }
}

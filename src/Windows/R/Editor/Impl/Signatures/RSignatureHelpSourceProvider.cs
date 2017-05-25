// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Signatures {
    [Export(typeof(ISignatureHelpSourceProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [Name("R Signature Source Provider")]
    [Order(Before = "default")]
    sealed class RSignatureHelpSourceProvider : ISignatureHelpSourceProvider {
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public RSignatureHelpSourceProvider(ICoreShell shell) {
            _shell = shell;
        }

        public ISignatureHelpSource TryCreateSignatureHelpSource(ITextBuffer textBuffer) {
            var helpSource = textBuffer.GetService<RSignatureHelpSource>();
            return helpSource ?? new RSignatureHelpSource(textBuffer, _shell.Services);
        }
    }
}

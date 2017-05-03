// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.QuickInfo {
    [Export(typeof(IQuickInfoSourceProvider))]
    [Name("R QuickInfo Source")]
    [Order(Before = "Default Quick Info Presenter")]
    [ContentType(RContentTypeDefinition.ContentType)]
    sealed class QuickInfoSourceProvider : IQuickInfoSourceProvider {
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public QuickInfoSourceProvider(ICoreShell shell) {
            _shell = shell;
        }

        public IQuickInfoSource TryCreateQuickInfoSource(ITextBuffer textBuffer) 
            => textBuffer.Properties.GetOrCreateSingletonProperty(() => new QuickInfoSource(textBuffer, _shell.Services));
    }
}

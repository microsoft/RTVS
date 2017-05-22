// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Navigation.Peek {
    [Export(typeof(IPeekableItemSourceProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [Name("R Peekable Item Provider")]
    [SupportsStandaloneFiles(true)]
    internal sealed class PeekableItemSourceProvider : IPeekableItemSourceProvider {
        private readonly IPeekResultFactory _peekResultFactory;
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public PeekableItemSourceProvider(IPeekResultFactory peekResultFactory, ICoreShell shell) {
            _peekResultFactory = peekResultFactory;
            _shell = shell;
        }

        public IPeekableItemSource TryCreatePeekableItemSource(ITextBuffer textBuffer)
            => textBuffer.Properties.GetOrCreateSingletonProperty(() => new PeekableItemSource(textBuffer, _peekResultFactory, _shell.Services));
    }
}

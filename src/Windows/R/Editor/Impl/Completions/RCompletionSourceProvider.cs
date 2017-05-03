// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.Completions {
    /// <summary>
    /// Completion source for Visual Studio core editor
    /// </summary>
    [Export(typeof(ICompletionSourceProvider))]
    [ContentType(RContentTypeDefinition.ContentType)]
    [Name("R Completion Source Provider")]
    [Order(Before = "default")]
    internal class RCompletionSourceProvider : ICompletionSourceProvider {
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public RCompletionSourceProvider(ICoreShell shell) {
            _shell = shell;
        }

        public ICompletionSource TryCreateCompletionSource(ITextBuffer textBuffer)
            => textBuffer.Properties.GetOrCreateSingletonProperty(() => new RCompletionSource(textBuffer, _shell.Services));
    }
}

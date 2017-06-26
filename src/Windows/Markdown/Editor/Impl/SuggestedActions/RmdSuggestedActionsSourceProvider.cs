// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.SuggestedActions {
    [Export(typeof(ISuggestedActionsSourceProvider))]
    [Name("RmdSuggestedActionSourceProvider")]
    [ContentType(MdContentTypeDefinition.ContentType)]
    internal sealed class RmdSuggestedActionsSourceProvider : ISuggestedActionsSourceProvider {
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public RmdSuggestedActionsSourceProvider(ICoreShell shell) {
            _shell = shell;
        }

        public ISuggestedActionsSource CreateSuggestedActionsSource(ITextView textView, ITextBuffer textBuffer)
            => textView.Properties.GetOrCreateSingletonProperty(() => RmdSuggestedActionsSource.Create(textView, textBuffer, _shell.Services));
    }
}

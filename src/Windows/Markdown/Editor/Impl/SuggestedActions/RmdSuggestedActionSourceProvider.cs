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
    [Name("RSuggestedActionSourceProvider")]
    [ContentType(MdContentTypeDefinition.ContentType)]
    internal sealed class RmdSuggestedActionSourceProvider : ISuggestedActionsSourceProvider {
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public RmdSuggestedActionSourceProvider(ICoreShell shell) {
            _shell = shell;
        }

        public ISuggestedActionsSource CreateSuggestedActionsSource(ITextView textView, ITextBuffer textBuffer)
            => textView.Properties.GetOrCreateSingletonProperty(() => RmdSuggestedActionSource.FromViewAndBuffer(textView, textBuffer, _shell));
    }
}

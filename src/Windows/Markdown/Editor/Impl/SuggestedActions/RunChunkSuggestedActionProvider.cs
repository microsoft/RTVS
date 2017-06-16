// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.SuggestedActions;
using Microsoft.Markdown.Editor.ContentTypes;
using Microsoft.Markdown.Editor.Settings;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.Markdown.Editor.SuggestedActions {
    [Export(typeof(ISuggestedActionProvider))]
    [ContentType(MdContentTypeDefinition.ContentType)]
    [Name("R Markdown Code Run Suggested Action Provider")]
    internal sealed class RunChunkSuggestedActionProvider : ISuggestedActionProvider {
        private readonly IRMarkdownEditorSettings _settings;

        [ImportingConstructor]
        public RunChunkSuggestedActionProvider(ICoreShell coreShell) {
            _settings = coreShell.Services.GetService<IRMarkdownEditorSettings>();
        }

        public IEnumerable<ISuggestedAction> GetSuggestedActions(ITextView textView, ITextBuffer textBuffer, int bufferPosition) {
            return new ISuggestedAction[] {
                new RunChunkSuggestedAction(textView, textBuffer, bufferPosition),
                new RunChunksAboveSuggestedAction(textView, textBuffer, bufferPosition),
            };
        }

        public bool HasSuggestedActions(ITextView textView, ITextBuffer textBuffer, int bufferPosition) 
            => !_settings.AutomaticSync && textView.IsCaretInRCode();
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.R.Components.ContentTypes;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Editor.SuggestedActions {
    [Export(typeof(ISuggestedActionsSourceProvider))]
    [Name("RSuggestedActionSourceProvider")]
    [ContentType(RContentTypeDefinition.ContentType)]
    internal sealed class RSuggestedActionSourceProvider : ISuggestedActionsSourceProvider {
        public ISuggestedActionsSource CreateSuggestedActionsSource(ITextView textView, ITextBuffer textBuffer) {
            return RSuggestedActionSource.FromViewAndBuffer(textView, textBuffer);
        }
    }
}

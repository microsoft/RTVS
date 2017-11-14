// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Formatting;
using Microsoft.Languages.Editor.Selection;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.R.LanguageServer.Formatting {
    internal sealed class IncrementalTextChangeHandler : WhitespaceTextChangeHandler, IIncrementalWhitespaceChangeHandler {
        /// <summary>
        /// Incrementally applies whitespace change to the buffer 
        /// having old and new tokens produced from the 'before formatting' 
        /// and 'after formatting' versions of the same text.
        /// </summary>
        /// <param name="editorBuffer">Text buffer to apply changes to</param>
        /// <param name="oldTextProvider">Text provider of the text fragment before formatting</param>
        /// <param name="newTextProvider">Text provider of the formatted text</param>
        /// <param name="oldTokens">Tokens from the 'before' text fragment</param>
        /// <param name="newTokens">Tokens from the 'after' text fragment</param>
        /// <param name="formatRange">Range that is being formatted in the text buffer</param>
        /// <param name="transactionName">Not used in VS Code</param>
        /// <param name="selectionTracker">Not used in VS Code</param>
        /// <param name="additionalAction">
        /// Action to perform after changes are applies by undo unit is not yet closed.
        /// </param>
        public void ApplyChange(
            IEditorBuffer editorBuffer,
            ITextProvider oldTextProvider,
            ITextProvider newTextProvider,
            IReadOnlyList<ITextRange> oldTokens,
            IReadOnlyList<ITextRange> newTokens,
            ITextRange formatRange,
            string transactionName,
            ISelectionTracker selectionTracker,
            Action additionalAction = null) {

            Debug.Assert(oldTokens.Count == newTokens.Count);
            if (oldTokens.Count != newTokens.Count) {
                return;
            }

            var edits = CalculateChanges(oldTextProvider, newTextProvider, oldTokens, newTokens, formatRange);
            foreach(var e in edits) {
                if(string.IsNullOrEmpty(e.NewText)) {
                    editorBuffer.Delete(e.Range);
                } else if(e.Range.Length > 0) {
                    editorBuffer.Replace(e.Range, e.NewText);
                } else {
                    editorBuffer.Insert(e.Range.Start, e.NewText);
                }
            }

            additionalAction?.Invoke();
         }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using LanguageServer.VsCode.Contracts;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Formatting;
using Microsoft.Languages.Editor.Selection;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.LanguageServer.Extensions;

namespace Microsoft.R.LanguageServer.Formatting {
    internal sealed class IncrementalTextChangeHandler : IIncrementalWhitespaceChangeHandler {
        public TextEdit[] Result { get; private set; } = new TextEdit[0];

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
        /// <param name="transactionName">Name of the undo transaction to open</param>
        /// <param name="selectionTracker">
        /// Selection tracker object that will save, 
        /// track and restore selection after changes have been applied.</param>
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

            var snapshot = editorBuffer.CurrentSnapshot;
            if (oldTokens.Count == 0) {
                Result = new[] {
                    new TextEdit {
                        NewText = newTextProvider.GetText(TextRange.FromBounds(0, newTextProvider.Length)),
                        Range = snapshot.ToLineRange(formatRange.Start, formatRange.End)
                    }
                };
                return;
            }

            // Replace whitespace between tokens in reverse so relative positions match
            var edits = new List<TextEdit>();
            var oldEnd = oldTextProvider.Length;
            var newEnd = newTextProvider.Length;
            for (var i = newTokens.Count - 1; i >= 0; i--) {
                var oldText = oldTextProvider.GetText(TextRange.FromBounds(oldTokens[i].End, oldEnd));
                var newText = newTextProvider.GetText(TextRange.FromBounds(newTokens[i].End, newEnd));
                if (oldText != newText) {
                    var range = new TextRange(formatRange.Start + oldTokens[i].End, oldEnd - oldTokens[i].End);
                    edits.Add(new TextEdit {
                        Range = snapshot.ToLineRange(range.Start, range.End),
                        NewText = newText
                    });

                }
                oldEnd = oldTokens[i].Start;
                newEnd = newTokens[i].Start;
            }

            var r = new TextRange(formatRange.Start, oldEnd);
            edits.Add(new TextEdit {
                NewText = newTextProvider.GetText(TextRange.FromBounds(0, newEnd)),
                Range = snapshot.ToLineRange(r.Start, r.End)
             });

            additionalAction?.Invoke();
            Result = edits.ToArray();
        }
    }
}

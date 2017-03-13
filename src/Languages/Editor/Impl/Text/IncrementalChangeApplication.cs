// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Selection;
using Microsoft.Languages.Editor.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.Languages.Editor.Text {
    public static class IncrementalTextChangeApplication {
#if _NOT_USED_
        /// Takes current text buffer and new text then builds list of changed
        /// regions and applies them to the buffer. This way we can avoid 
        /// destruction of bookmarks and other markers. Complete
        /// buffer replacement deletes all markers which causes 
        /// loss of bookmarks, breakpoints and other similar markers.
        public static void ApplyChange(
            ITextBuffer textBuffer,
            int position,
            int length,
            string newText,
            string transactionName,
            ISelectionTracker selectionTracker,
            int maxMilliseconds) {

            var snapshot = textBuffer.CurrentSnapshot;
            int oldLength = Math.Min(length, snapshot.Length - position);
            string oldText = snapshot.GetText(position, oldLength);

            var changes = TextChanges.BuildChangeList(oldText, newText, maxMilliseconds);
            if (changes != null && changes.Count > 0) {
                using (var selectionUndo = new SelectionUndo(selectionTracker, transactionName, automaticTracking: false)) {
                    using (ITextEdit edit = textBuffer.CreateEdit()) {
                        // Replace ranges in reverse so relative positions match
                        for (int i = changes.Count - 1; i >= 0; i--) {
                            TextChange tc = changes[i];
                            edit.Replace(tc.Position + position, tc.Length, tc.NewText);
                        }

                        edit.Apply();
                    }
                }
            }
        }
#endif
        /// <summary>
        /// Incrementally applies whitespace change to the buffer 
        /// having old and new tokens produced from the 'before formatting' 
        /// and 'after formatting' versions of the same text.
        /// </summary>
        /// <param name="textBuffer">Text buffer to apply changes to</param>
        /// <param name="newTextProvider">Text provider of the text fragment before formatting</param>
        /// <param name="newTextProvider">Text provider of the formatted text</param>
        /// <param name="oldTokens">Tokens from the 'before' text fragment</param>
        /// <param name="newTokens">Tokens from the 'after' text fragment</param>
        /// <param name="formatRange">Range that is being formatted in the text buffer</param>
        /// <param name="transactionName">Name of the undo transaction to open</param>
        /// <param name="selectionTracker">Selection tracker object that will save, track
        /// <param name="additionalAction">Action to perform after changes are applies by undo unit is not yet closed.</param>
        /// and restore selection after changes have been applied.</param>
        public static void ApplyChangeByTokens(
            ITextBuffer textBuffer,
            ITextProvider oldTextProvider,
            ITextProvider newTextProvider,
            IReadOnlyList<ITextRange> oldTokens,
            IReadOnlyList<ITextRange> newTokens,
            ITextRange formatRange,
            string transactionName,
            ISelectionTracker selectionTracker,
            IEditorShell editorShell,
            Action additionalAction = null) {

            Debug.Assert(oldTokens.Count == newTokens.Count);
            if (oldTokens.Count == newTokens.Count) {
                ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
                using (CreateSelectionUndo(selectionTracker, editorShell, transactionName)) {
                    using (ITextEdit edit = textBuffer.CreateEdit()) {
                        if (oldTokens.Count > 0) {
                            // Replace whitespace between tokens in reverse so relative positions match
                            int oldEnd = oldTextProvider.Length;
                            int newEnd = newTextProvider.Length;
                            string oldText, newText;
                            for (int i = newTokens.Count - 1; i >= 0; i--) {
                                oldText = oldTextProvider.GetText(TextRange.FromBounds(oldTokens[i].End, oldEnd));
                                newText = newTextProvider.GetText(TextRange.FromBounds(newTokens[i].End, newEnd));
                                if (oldText != newText) {
                                    edit.Replace(formatRange.Start + oldTokens[i].End, oldEnd - oldTokens[i].End, newText);
                                }
                                oldEnd = oldTokens[i].Start;
                                newEnd = newTokens[i].Start;
                            }
                            newText = newTextProvider.GetText(TextRange.FromBounds(0, newEnd));
                            edit.Replace(formatRange.Start, oldEnd, newText);
                        } else {
                            string newText = newTextProvider.GetText(TextRange.FromBounds(0, newTextProvider.Length));
                            edit.Replace(formatRange.Start, formatRange.Length, newText);
                        }
                        edit.Apply();
                        additionalAction?.Invoke();
                    }
                }
            }
        }

        private static IDisposable CreateSelectionUndo(ISelectionTracker selectionTracker, IEditorShell editorShell, string transactionName) {
            if (editorShell.IsUnitTestEnvironment) {
                return Disposable.Empty;
            }

            var textBufferUndoManagerProvider = editorShell.GlobalServices.GetService<ITextBufferUndoManagerProvider>();
            return new SelectionUndo(selectionTracker, textBufferUndoManagerProvider, transactionName, automaticTracking: false);
        }
    }
}

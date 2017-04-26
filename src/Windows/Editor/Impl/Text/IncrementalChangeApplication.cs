// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Formatting;
using Microsoft.Languages.Editor.Selection;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;

namespace Microsoft.Languages.Editor.Text {
    [Export(typeof(IIncrementalWhitespaceChangeHandler))]
    public sealed class IncrementalTextChangeApplication: IIncrementalWhitespaceChangeHandler {
        private readonly IServiceContainer _services;

        [ImportingConstructor]
        public IncrementalTextChangeApplication(ICoreShell coreShell) {
            _services = coreShell.Services;
        }

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
            if (oldTokens.Count == newTokens.Count) {
                using (CreateSelectionUndo(selectionTracker, _services, transactionName)) {
                    var textBuffer = editorBuffer.As<ITextBuffer>();
                    using (var edit = textBuffer.CreateEdit()) {
                        if (oldTokens.Count > 0) {
                            // Replace whitespace between tokens in reverse so relative positions match
                            var oldEnd = oldTextProvider.Length;
                            var newEnd = newTextProvider.Length;
                            string newText;
                            for (var i = newTokens.Count - 1; i >= 0; i--) {
                                var oldText = oldTextProvider.GetText(TextRange.FromBounds(oldTokens[i].End, oldEnd));
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
                            var newText = newTextProvider.GetText(TextRange.FromBounds(0, newTextProvider.Length));
                            edit.Replace(formatRange.Start, formatRange.Length, newText);
                        }
                        edit.Apply();
                        additionalAction?.Invoke();
                    }
                }
            }
        }

        private static IDisposable CreateSelectionUndo(ISelectionTracker selectionTracker, IServiceContainer services, string transactionName) {
            var textBufferUndoManagerProvider = services.GetService<ITextBufferUndoManagerProvider>();
            if (textBufferUndoManagerProvider != null) {
                return new SelectionUndo(selectionTracker, textBufferUndoManagerProvider, transactionName, automaticTracking: false);
            }
            return Disposable.Empty;
        }
    }
}

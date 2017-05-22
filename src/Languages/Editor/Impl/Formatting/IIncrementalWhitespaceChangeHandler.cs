// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Selection;
using Microsoft.Languages.Editor.Text;

namespace Microsoft.Languages.Editor.Formatting {
    public interface IIncrementalWhitespaceChangeHandler {
        /// <summary>
        /// Incrementally applies whitespace change to the buffer having old and new tokens (ranges) 
        /// produced from the 'before formatting' and 'after formatting' versions of the same text.
        /// Minimizes changes to the editor buffer and preserves selection and caret position
        /// in range formatting, automatinc formatting and brace insertion/positioning.
        /// </summary>
        /// <param name="editorBuffer">Text buffer to apply changes to</param>
        /// <param name="oldTextProvider">Text provider of the text fragment before formatting</param>
        /// <param name="newTextProvider">Text provider of the formatted text</param>
        /// <param name="oldTokens">Tokens from the 'before' text fragment</param>
        /// <param name="newTokens">Tokens from the 'after' text fragment</param>
        /// <param name="formatRange">Range that is being formatted in the text buffer</param>
        /// <param name="transactionName">Name of the undo transaction to open</param>
        /// <param name="selectionTracker">
        /// Selection tracker object that will save, track and restore selection 
        /// after changes have been applied
        /// </param>
        /// <param name="additionalAction">
        /// Action to perform after changes are applies but the associated undo 
        /// unit is not yet closed.
        /// </param>
        void ApplyChange(
            IEditorBuffer editorBuffer,
            ITextProvider oldTextProvider,
            ITextProvider newTextProvider,
            IReadOnlyList<ITextRange> oldTokens,
            IReadOnlyList<ITextRange> newTokens,
            ITextRange formatRange,
            string transactionName,
            ISelectionTracker selectionTracker,
            Action additionalAction = null);
    }
}

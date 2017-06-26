// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.SuggestedActions {
    /// <summary>
    /// Export this interface via MEF for HTML or derived content type
    /// </summary>
    public interface ISuggestedActionProvider {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="textView">Text view</param>
        /// <param name="textBuffer">Text buffer</param>
        /// <param name="caretPosition">Caret position in text buffer coordinates</param>
        /// <returns>Whether any suggested actions are currently applicable</returns>
        bool HasSuggestedActions(ITextView textView, ITextBuffer textBuffer, int caretPosition);

        /// <summary>
        /// Tries to create suggested actions for a given element and position in the element
        /// </summary>
        /// <param name="textView">Text view</param>
        /// <param name="textBuffer">Text buffer</param>
        /// <param name="caretPosition">Caret position in text buffer coordinates</param>
        /// <returns>A collection of applicable suggested actions</returns>
        IEnumerable<ISuggestedAction> GetSuggestedActions(ITextView textView, ITextBuffer textBuffer, int caretPosition);
    }
}

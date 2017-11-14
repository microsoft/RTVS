// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;

namespace Microsoft.R.LanguageServer.Formatting {
    public class WhitespaceTextChangeHandler {
        protected TextEdit[] CalculateChanges(
            ITextProvider oldTextProvider,
            ITextProvider newTextProvider,
            IReadOnlyList<ITextRange> oldTokens,
            IReadOnlyList<ITextRange> newTokens,
            ITextRange formatRange) {

            Debug.Assert(oldTokens.Count == newTokens.Count);
            if (oldTokens.Count != newTokens.Count) {
                return new TextEdit[0];
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
                    edits.Add(new TextEdit(range, newText));
                }
                oldEnd = oldTokens[i].Start;
                newEnd = newTokens[i].Start;
            }

            var r = new TextRange(formatRange.Start, oldEnd);
            var n = newTextProvider.GetText(TextRange.FromBounds(0, newEnd));
            if (r.Length > 0 || !string.IsNullOrEmpty(n)) {
                edits.Add(new TextEdit(r, n));
            }

            return edits.ToArray();
        }
    }
}

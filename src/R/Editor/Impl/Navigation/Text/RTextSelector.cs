// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Navigation.Text {
    internal static class RTextStructure {
        public static Span? GetWordSpan(ITextSnapshot snapshot, int position) {
            ITextSnapshotLine line = snapshot.GetLineFromPosition(position);
            if (line.Length == 0) {
                return null;
            }

            var text = line.GetText();
            var tokenizer = new RTokenizer();
            var tokens = tokenizer.Tokenize(text);
            var positionInLine = position - line.Start;

            var token = tokens.FirstOrDefault(t => t.Contains(positionInLine));
            if (token != null && token.TokenType != RTokenType.String && token.TokenType != RTokenType.Comment) {
                return new Span(token.Start + line.Start, token.Length);
            }
            return GetWordSpan(text, line.Start, positionInLine);
        }

        private static Span GetWordSpan(string text, int lineStart, int position) {
            // Select at least one character. Selection is a bit trickly since
            // poisition is at character while actual user click and caret position
            // is between characters. So the position passed can be before or after 
            // the actual caret position.
            if (text.Length == 0) {
                // Nothing to select
                return new Span(lineStart, 0);
            }

            int start, end;
            for (start = Math.Min(position, text.Length - 1); start >= 0; start--) {
                if (IsSeparator(text[start])) {
                    if (start < position) {
                        start++;
                    }
                    break;
                }
            }

            for (end = position; end < text.Length; end++) {
                if (IsSeparator(text[end])) {
                    break;
                }
            }

            end = Math.Min(Math.Max(end, start + 1), text.Length);
            start = Math.Max(0, Math.Min(start, end - 1));

            return Span.FromBounds(start + lineStart, end + lineStart);
        }

        private static bool IsSeparator(char ch) {
            return !char.IsLetterOrDigit(ch);
        }
    }
}

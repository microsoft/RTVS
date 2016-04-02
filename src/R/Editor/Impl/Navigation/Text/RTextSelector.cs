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
            if (token != null && token.TokenType != RTokenType.String) {
                return new Span(token.Start + line.Start, token.Length);
            }
            return GetWordSpan(text, line.Start, positionInLine);
        }

        private static Span GetWordSpan(string text, int lineStart, int position) {
            // Select at least one character
            int start, end;
            for (start = position; start >= 0; start--) {
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

            return Span.FromBounds(start + lineStart, Math.Min(end, text.Length) + lineStart);
        }

        private static bool IsSeparator(char ch) {
            return !char.IsLetterOrDigit(ch);
        }
    }
}

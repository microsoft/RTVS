// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Navigation.Text {
    internal static class RTextStructure {
        public static Span? GetWordSpan(ITextSnapshot snapshot, int position) {
            ITextSnapshotLine line = snapshot.GetLineFromPosition(position);
            // Tokenize current line
            if (line != null) {
                var text = line.GetText();
                var tokenizer = new RTokenizer();
                var tokens = tokenizer.Tokenize(text);
                var positionInLine = position - line.Start;

                var token = tokens.FirstOrDefault(t => t.Contains(positionInLine));
                if (token != null) {
                    if (token.TokenType == RTokenType.String) {
                        // Select word inside string
                        return GetWordSpan(text, line.Start, positionInLine);
                    } else {
                        return new Span(token.Start + line.Start, token.Length);
                    }
                }
            }
            return null;
        }

        private static Span GetWordSpan(string text, int lineStart, int position) {
            int start = position;
            int end = position;
            for (start = position; start >= 0; start--) {
                if (IsSeparator(text[start])) {
                    start++;
                    break;
                }
            }
            for (end = position + 1; end < text.Length; end++) {
                if (IsSeparator(text[end])) {
                    break;
                }
            }
            return Span.FromBounds(start + lineStart, end + lineStart);
        }

        private static bool IsSeparator(char ch) {
            return char.IsWhiteSpace(ch) || ch == '\'' || ch == '\"' || ch == '\\';
        }
    }
}

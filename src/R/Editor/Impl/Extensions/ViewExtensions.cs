// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Editor {
    public static class ViewExtensions {
        /// <summary>
        /// Extracts identifier sequence at the caret location.
        /// Fetches parts of 'abc$def' rather than tne entire expression.
        /// If there is selection, returns complete selected item as is.
        /// </summary>
        public static string GetIdentifierUnderCaret(this IEditorView view, out ITextRange span) {
            if (!view.Caret.InVirtualSpace) {
                if (view.Selection.Mode == SelectionMode.Stream) {
                    var position = view.Caret.Position;
                    int caretPosition = -1;
                    span = view.Selection.SelectedRange;
                    if (span.Length > 0) {
                        return view.EditorBuffer.CurrentSnapshot.GetText(span);
                    }
                    var line = position.GetContainingLine();
                    caretPosition = caretPosition >= 0 ? caretPosition : position.Position;
                    var item = GetItemAtPosition(line, caretPosition, x => x == RTokenType.Identifier, out span);
                    if (string.IsNullOrEmpty(item)) {
                        item = view.GetItemBeforeCaret(out span, x => x == RTokenType.Identifier);
                    }
                    return item;
                }
            }
            span = TextRange.EmptyRange;
            return string.Empty;
        }

        /// <summary>
        /// Extracts identifier or a keyword before caret. Typically used when inserting
        /// expansions (aka code snippets) at the caret location.
        /// </summary>
        public static string GetItemBeforeCaret(this IEditorView view, out ITextRange span, Func<RTokenType, bool> tokenTypeCheck = null) {
            if (!view.Caret.InVirtualSpace) {
                var position = view.Caret.Position;
                var line = position.GetContainingLine();
                tokenTypeCheck = tokenTypeCheck ?? new Func<RTokenType, bool>((x) => x == RTokenType.Identifier);
                if (position.Position > line.Start) {
                    return GetItemAtPosition(line, position.Position - 1, tokenTypeCheck, out span);
                }
            }
            span = TextRange.EmptyRange;
            return string.Empty;
        }

        public static string GetItemAtPosition(IEditorLine line, int position, Func<RTokenType, bool> tokenTypeCheck, out ITextRange span) {
            string lineText = line.GetText();
            var offset = 0;
            var positionInTokens = position - line.Start;
            var tokenizer = new RTokenizer();
            var tokens = tokenizer.Tokenize(lineText);
            var tokenIndex = tokens.GetItemContaining(positionInTokens);
            if (tokenIndex >= 0) {
                var token = tokens[tokenIndex];
                if (token.TokenType == RTokenType.Comment) {
                    // Tokenize inside comment since we do want F1 to work inside 
                    // commented out code, code samples or Roxygen blocks.
                    positionInTokens -= token.Start;
                    var positionAfterHash = token.Start + 1;
                    tokens = tokenizer.Tokenize(lineText.Substring(positionAfterHash, token.Length - 1));
                    tokenIndex = tokens.GetItemContaining(positionInTokens);
                    if (tokenIndex >= 0) {
                        token = tokens[tokenIndex];
                        offset = positionAfterHash;
                    }
                }
                if (tokenTypeCheck(token.TokenType)) {
                    var start = token.Start + offset;
                    var end = Math.Min(start + token.Length, line.End);
                    span = TextRange.FromBounds(line.Start + start, line.Start + end); // return view span
                    return lineText.Substring(start, end - start);
                }
            }

            span = TextRange.EmptyRange;
            return string.Empty;
        }

        /// <summary>
        /// Extracts complete variable name under the caret. In '`abc`$`def` returns 
        /// the complete expression rather than its parts. Typically used to get data 
        /// for completion of variable members as in when user typed 'abc$def$'
        /// Since method does not perform semantic analysis, it does not guaratee 
        /// syntactically correct expression, it may return 'a$$b'.
        /// </summary>
        public static string GetVariableNameBeforeCaret(this IEditorView view) {
            if (view.Caret.InVirtualSpace) {
                return string.Empty;
            }
            var position = view.Caret.Position;
            var line = position.GetContainingLine();

            // For performance reasons we won't be using AST here
            // since during completion it is most probably damaged.
            string lineText = line.GetText();
            var tokenizer = new RTokenizer();
            var tokens = tokenizer.Tokenize(lineText);

            var tokenPosition = position.Position - line.Start;
            var index = tokens.GetFirstItemBeforePosition(tokenPosition);
            // Preceding token must be right next to caret position
            if (index < 0 || tokens[index].End < tokenPosition || !IsVariableNameToken(lineText, tokens[index])) {
                return string.Empty;
            }

            if (index == 0) {
                return IsVariableNameToken(lineText, tokens[0]) ? lineText.Substring(tokens[0].Start, tokens[0].Length) : string.Empty;
            }

            // Walk back through tokens allowing identifier and specific
            // operator tokens. No whitespace is permitted between tokens.
            // We have at least 2 tokens here.
            int i = index;
            for (; i > 0; i--) {
                var precedingToken = tokens[i - 1];
                var currentToken = tokens[i];
                if (precedingToken.End < currentToken.Start || !IsVariableNameToken(lineText, precedingToken)) {
                    break;
                }
            }

            return lineText.Substring(tokens[i].Start, tokens[index].End - tokens[i].Start);
        }

        private static bool IsVariableNameToken(string lineText, RToken token) {
            if (token.TokenType == RTokenType.Identifier) {
                return true;
            }
            if (token.TokenType == RTokenType.Operator) {
                if (token.Length == 1) {
                    return lineText[token.Start] == '$' || lineText[token.Start] == '@';
                } else if (token.Length == 2) {
                    return lineText[token.Start] == ':' && lineText[token.Start + 1] == ':';
                }
            }
            return false;
        }
    }
}

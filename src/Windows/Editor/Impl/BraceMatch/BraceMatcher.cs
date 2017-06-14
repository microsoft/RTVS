// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.Languages.Editor.BraceMatch.Definitions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.BraceMatch {
    public abstract class BraceMatcher<TokenClassT, TokenTypeT> : IBraceMatcher
        where TokenClassT : IToken<TokenTypeT> {
        public enum BraceType {
            Curly,
            Square,
            Parenthesis,
            Unknown
        }

        // Overriding classes should provide actual values in their static constructors.
        // Reserving space for three items, as the most-commonly used braces are Curly, Square, and Parenthesis.
        protected static Dictionary<BraceType, BraceTokenPair<TokenTypeT>> BraceTypeToTokenTypeMap = new Dictionary<BraceType, BraceTokenPair<TokenTypeT>>(3);

        public ITextView TextView { get; private set; }
        public ITextBuffer TextBuffer { get; private set; }

        private readonly IComparer<TokenTypeT> _tokenComparer;

        protected BraceMatcher(ITextView textView, ITextBuffer textBuffer, IComparer<TokenTypeT> tokenComparer) {
            TextView = textView;
            TextBuffer = textBuffer;
            _tokenComparer = tokenComparer;
        }

        public bool GetBracesFromPosition(ITextSnapshot snapshot, int currentPosition, bool extendSelection, out int startPosition, out int endPosition) {
            startPosition = 0;
            endPosition = 0;

            if (snapshot != TextBuffer.CurrentSnapshot || snapshot.Length == 0) {
                return false;
            }

            var braceType = BraceType.Unknown;

            var ch = '\0';
            var validCharacter = false;
            var searchPosition = currentPosition;
            var reversed = false;

            if (currentPosition < snapshot.Length) {
                ch = snapshot[currentPosition];
                validCharacter = GetMatchingBraceType(ch, out braceType, out reversed);
            }

            if (!validCharacter && currentPosition > 0) {
                ch = snapshot[currentPosition - 1];
                validCharacter = GetMatchingBraceType(ch, out braceType, out reversed);
                searchPosition--;
            }

            if (!validCharacter) {
                return false;
            }

            return GetLanguageBracesFromPosition(braceType, searchPosition, reversed, out startPosition, out endPosition);
        }

        public static bool IsSupportedBraceType(BraceType braceType) {
            return BraceTypeToTokenTypeMap.ContainsKey(braceType);
        }

        protected abstract IReadOnlyTextRangeCollection<TokenClassT> GetTokens(int start, int length);

        public virtual bool GetLanguageBracesFromPosition(
            BraceType braceType,
            int position, bool reversed, out int start, out int end) {
            var startTokenType = BraceTypeToTokenTypeMap[braceType].OpenBrace;
            var endTokenType = BraceTypeToTokenTypeMap[braceType].CloseBrace;
            var tokens = GetTokens(0, TextBuffer.CurrentSnapshot.Length);

            start = -1;
            end = -1;

            var stack = new Stack<TokenTypeT>();

            var startIndex = -1;
            for (var i = 0; i < tokens.Count; i++) {
                if (tokens[i].Start == position) {
                    startIndex = i;
                    break;
                }
            }

            if (startIndex < 0) {
                return false;
            }

            if (_tokenComparer.Compare(tokens[startIndex].TokenType, startTokenType) != 0 && _tokenComparer.Compare(tokens[startIndex].TokenType, endTokenType) != 0) {
                return false;
            }

            if (!reversed) {
                for (var i = startIndex; i < tokens.Count; i++) {
                    var token = tokens[i];

                    if (token.TokenType.Equals(startTokenType)) {
                        stack.Push(token.TokenType);
                    } else if (_tokenComparer.Compare(token.TokenType, endTokenType) == 0) {
                        if (stack.Count > 0) {
                            stack.Pop();
                        }

                        if (stack.Count == 0) {
                            start = tokens[startIndex].Start;
                            end = token.Start;
                            return true;
                        }
                    }
                }
            } else {
                for (var i = startIndex; i >= 0; i--) {
                    var token = tokens[i];

                    if (_tokenComparer.Compare(token.TokenType, endTokenType) == 0) {
                        stack.Push(token.TokenType);
                    } else if (_tokenComparer.Compare(token.TokenType, startTokenType) == 0) {
                        if (stack.Count > 0) {
                            stack.Pop();
                        }

                        if (stack.Count == 0) {
                            start = token.Start;
                            end = token.Start;

                            end = tokens[startIndex].Start;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool GetMatchingBraceType(char ch, out BraceType braceType, out bool reversed) {
            braceType = BraceType.Unknown;
            reversed = false;

            switch (ch) {
                case '{':
                case '}':
                    braceType = BraceType.Curly;
                    reversed = ch == '}';
                    break;

                case '(':
                case ')':
                    braceType = BraceType.Parenthesis;
                    reversed = ch == ')';
                    break;

                case '[':
                case ']':
                    braceType = BraceType.Square;
                    reversed = ch == ']';
                    break;
            }

            if (braceType != BraceType.Unknown && !IsSupportedBraceType(braceType)) {
                braceType = BraceType.Unknown;
                reversed = false;
            }

            return braceType != BraceType.Unknown;
        }
    }
}

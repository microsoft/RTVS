using System;
using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.Languages.Editor.BraceMatch.Definitions;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.BraceMatch {
    public class TokenBasedBraceMatcher<TTokenClass, TTokenType> : IBraceMatcher where TTokenClass : IToken<TTokenType> {
        private ReadOnlyTextRangeCollection<TTokenClass> _tokens;
        private Func<TTokenClass, bool> _isPunctuation;

        public TokenBasedBraceMatcher(ReadOnlyTextRangeCollection<TTokenClass> tokens, Func<TTokenClass, bool> isPunctuation) {
            _tokens = tokens;
            _isPunctuation = isPunctuation;
        }

        public bool GetBracesFromPosition(ITextSnapshot snapshot, int currentPosition, bool extendSelection, out int start, out int end) {
            return BraceMatch<TTokenClass, TTokenType>.GetBracesFromPosition(snapshot, _tokens, _isPunctuation, currentPosition, out start, out end);
        }
    }

    public static class BraceMatch<TTokenClass, TTokenType> where TTokenClass : IToken<TTokenType> {
        public static bool GetBracesFromPosition(ITextSnapshot snapshot, ReadOnlyTextRangeCollection<TTokenClass> tokens, Func<TTokenClass, bool> isPunctuation, int position, out int start, out int end) {
            start = -1;
            end = -1;

            var stack = new Stack<char>();
            int index = -1;

            var ch = GetTokenCharAtPosition(snapshot, tokens, isPunctuation, position, out index);
            var match = GetMatchingBraceChar(ch);

            if (match == '\0' && position > 0) {
                ch = GetTokenCharAtPosition(snapshot, tokens, isPunctuation, position - 1, out index);
                match = GetMatchingBraceChar(ch);
            }

            if (ch == '{' || ch == '(' || ch == '[') {
                for (int i = index + 1; i < tokens.Count; i++) {
                    if (isPunctuation(tokens[i])) {
                        if (IsBrace(snapshot, ch, tokens[i])) {
                            stack.Push(ch);
                        } else if (IsBrace(snapshot, match, tokens[i])) {
                            if (stack.Count == 0) {
                                start = tokens[index].Start;
                                end = tokens[i].Start;

                                return true;
                            } else {
                                stack.Pop();
                            }
                        }
                    }
                }
            } else if (ch == '}' || ch == ')' || ch == ']') {
                for (int i = index - 1; i >= 0; i--) {
                    if (isPunctuation(tokens[i])) {
                        if (IsBrace(snapshot, ch, tokens[i])) {
                            stack.Push(ch);
                        } else if (IsBrace(snapshot, match, tokens[i])) {
                            if (stack.Count == 0) {
                                end = tokens[index].Start;
                                start = tokens[i].Start;

                                return true;
                            } else {
                                stack.Pop();
                            }
                        }
                    }
                }
            }

            return false;
        }

        public static char GetTokenCharAtPosition(ITextSnapshot snapshot, ReadOnlyTextRangeCollection<TTokenClass> tokens, Func<TTokenClass, bool> isPunctuation, int position, out int index) {
            char ch = '\0';

            // Caret may be at or after punctuation, so we need to search at position, right before it
            index = tokens.GetItemAtPosition(position);
            if (index >= 0) {
                var token = tokens[index];
                if (isPunctuation(token)) {
                    if (position < snapshot.Length && snapshot.Length > 0)
                        ch = snapshot[position];
                }
            }

            return ch;
        }

        public static char GetMatchingBraceChar(char ch) {
            switch (ch) {
                case '{': return '}';
                case '}': return '{';
                case '(': return ')';
                case ')': return '(';
                case '[': return ']';
                case ']': return '[';
            }
            return '\0';
        }

        public static bool IsBrace(ITextSnapshot snapshot, char brace, TTokenClass token) {
            if (token.Start < snapshot.Length) {
                var candidate = snapshot[token.Start];
                return candidate == brace;
            }

            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.Languages.Editor.BraceMatch.Definitions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.Languages.Editor.BraceMatch {
    public abstract class BraceMatcher<TokenTypeT> : IBraceMatcher where TokenTypeT : struct {
        public enum BraceType {
            Curly,
            Square,
            Parenthesis,
            Unknown
        }

        // Overriding classes should provide actual values in their static constructors.
        // Reserving space for three items, as the most-commonly used braces are Curly, Square, and Parenthesis.
        protected static Dictionary<BraceType, Tuple<TokenTypeT, TokenTypeT>> BraceTypeToTokenTypeMap = new Dictionary<BraceType, Tuple<TokenTypeT, TokenTypeT>>(3);

        public ITextView TextView { get; private set; }
        public ITextBuffer TextBuffer { get; private set; }

        public BraceMatcher(ITextView textView, ITextBuffer textBuffer) {
            TextView = textView;
            TextBuffer = textBuffer;
        }

        public bool GetBracesFromPosition(ITextSnapshot snapshot, int currentPosition, bool extendSelection, out int startPosition, out int endPosition) {
            startPosition = 0;
            endPosition = 0;

            if (snapshot != TextBuffer.CurrentSnapshot || snapshot.Length == 0)
                return false;

            BraceType braceType = BraceType.Unknown;

            char ch = '\0';
            bool validCharacter = false;
            int searchPosition = currentPosition;
            bool reversed = false;

            if (currentPosition < snapshot.Length) {
                ch = snapshot[currentPosition];
                validCharacter = GetMatchingBraceType(ch, out braceType, out reversed);
            }

            if (!validCharacter && currentPosition > 0) {
                ch = snapshot[currentPosition - 1];
                validCharacter = GetMatchingBraceType(ch, out braceType, out reversed);
                searchPosition--;
            }

            if (!validCharacter)
                return false;

            return GetLanguageBracesFromPosition(braceType, searchPosition, reversed, out startPosition, out endPosition);
        }

        public static bool IsSupportedBraceType(BraceType braceType) {
            return BraceTypeToTokenTypeMap.ContainsKey(braceType);
        }

        public abstract bool GetLanguageBracesFromPosition(
            BraceType braceType,
            int position, bool reversed, out int start, out int end);

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

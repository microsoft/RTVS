// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Html.Core.Parser.Utility;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Html.Core.Parser.Tokens {
    internal enum NextTokenType {
        None,
        Unknown,
        Equals,     // =
        Letters,    // all-letters sequence
        Identifier, // begins with letter, but also contains digits
        Number,     // all digits with possible
        Tag
    }

    internal static class NextToken {
        public static NextTokenType PeekNextToken(HtmlCharStream cs, int tagEnd, out ITextRange range) {
            NextTokenType tokenType = NextTokenType.Unknown;
            int current = cs.Position;

            if (cs.IsEndOfStream() || cs.Position == tagEnd) {
                range = new TextRange();
                return NextTokenType.None;
            }

            int start = cs.Position;

            while (cs.IsWhiteSpace())
                cs.MoveToNextChar();

            if (cs.IsEndOfStream() || cs.Position == tagEnd) {
                range = TextRange.FromBounds(start, cs.Position);
                return NextTokenType.Unknown;
            }

            if (cs.IsAtTagDelimiter()) {
                tokenType = NextTokenType.Tag;
            } else if (cs.CurrentChar == '=') {
                tokenType = NextTokenType.Equals;
            } else {
                int digits = 0;
                bool firstLetter = false;
                int length = 0;
                int chars = 0;

                if (cs.IsAnsiLetter())
                    firstLetter = true;

                while (!cs.IsEndOfStream() && !cs.IsWhiteSpace() && !cs.IsAtTagDelimiter() && cs.CurrentChar != '=' && cs.Position < tagEnd) {
                    if (cs.IsAnsiLetter() || cs.CurrentChar == '_' || cs.CurrentChar == '-')
                        chars++;
                    else if (cs.IsDecimal() || cs.CurrentChar == '.')
                        digits++;

                    cs.MoveToNextChar();
                    length++;
                }

                if (length > 0) {
                    if (length == digits)
                        tokenType = NextTokenType.Number;
                    else if (length == chars)
                        tokenType = NextTokenType.Letters;
                    else if (firstLetter)
                        tokenType = NextTokenType.Identifier;
                }
            }

            range = TextRange.FromBounds(start, cs.Position);
            cs.Position = current;
            return tokenType;
        }
    }
}

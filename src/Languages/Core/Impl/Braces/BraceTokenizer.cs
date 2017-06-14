// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;
using static Microsoft.Languages.Core.Braces.BraceTokenType;

namespace Microsoft.Languages.Core.Braces {
    public sealed class BraceTokenizer {
        public IReadOnlyTextRangeCollection<BraceToken> Tokenize(string text) {
            var cs = new CharacterStream(text);
            var tokens = new TextRangeCollection<BraceToken>();
            while (!cs.IsEndOfStream()) {
                BraceTokenType? t = null;
                switch (cs.CurrentChar) {
                    case '(':
                        t = OpenBrace;
                        break;
                    case ')':
                        t = CloseBrace;
                        break;
                    case '{':
                        t = OpenCurly;
                        break;
                    case '}':
                        t = CloseCurly;
                        break;
                    case '[':
                        t = OpenBracket;
                        break;
                    case ']':
                        t = CloseBracket;
                        break;
                }
                if (t != null) {
                    tokens.Add(new BraceToken(cs.Position, 1, t.Value));
                }
                cs.MoveToNextChar();
            }
            return new ReadOnlyTextRangeCollection<BraceToken>(tokens);
        }
    }
}

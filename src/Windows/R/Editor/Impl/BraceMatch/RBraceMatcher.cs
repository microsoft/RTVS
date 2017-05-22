// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.BraceMatch;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.Tokens;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.BraceMatch {
    internal sealed class RBraceMatcher : BraceMatcher<RToken, RTokenType> {
        static RBraceMatcher() {
            BraceTypeToTokenTypeMap.Add(BraceType.Curly, new BraceTokenPair<RTokenType>(RTokenType.OpenCurlyBrace, RTokenType.CloseCurlyBrace));
            BraceTypeToTokenTypeMap.Add(BraceType.Parenthesis, new BraceTokenPair<RTokenType>(RTokenType.OpenBrace, RTokenType.CloseBrace));
            BraceTypeToTokenTypeMap.Add(BraceType.Square, new BraceTokenPair<RTokenType>(RTokenType.OpenSquareBracket, RTokenType.CloseSquareBracket));
        }

        public RBraceMatcher(ITextView textView, ITextBuffer textBuffer) : 
            base(textView, textBuffer, new TokenComparer()) {
        }

        protected override IReadOnlyTextRangeCollection<RToken> GetTokens(int start, int length) {
            var tokenizer = new RTokenizer();
            ITextProvider tp = new TextProvider(TextBuffer.CurrentSnapshot);
            var tokens = tokenizer.Tokenize(tp, 0, tp.Length);

            // In R there is [[ construct that comes as 'double square bracket' token.
            // Note that VS brace highlighter can only handle single-character braces.
            // Hence we'll massage token stream and replace double-bracket tokens but
            // a pair of square bracket tokens.
            var updated = new TextRangeCollection<RToken>();
            foreach (var t in tokens) {
                switch (t.TokenType) {
                    case RTokenType.OpenDoubleSquareBracket:
                        updated.Add(new RToken(RTokenType.OpenSquareBracket, t.Start, 1));
                        updated.Add(new RToken(RTokenType.OpenSquareBracket, t.Start + 1, 1));
                        break;
                    case RTokenType.CloseDoubleSquareBracket:
                        updated.Add(new RToken(RTokenType.CloseSquareBracket, t.Start, 1));
                        updated.Add(new RToken(RTokenType.CloseSquareBracket, t.Start + 1, 1));
                        break;

                    default:
                        updated.Add(t);
                        break;
                }
            }

            return updated;
        }

        class TokenComparer : IComparer<RTokenType> {
            public int Compare(RTokenType x, RTokenType y) => x.CompareTo(y);
        }
    }
}

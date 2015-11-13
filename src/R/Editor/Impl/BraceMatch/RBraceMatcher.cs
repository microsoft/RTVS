using System;
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
            BraceTypeToTokenTypeMap.Add(BraceType.Curly, new Tuple<RTokenType, RTokenType>(RTokenType.OpenCurlyBrace, RTokenType.CloseCurlyBrace));
            BraceTypeToTokenTypeMap.Add(BraceType.Parenthesis, new Tuple<RTokenType, RTokenType>(RTokenType.OpenBrace, RTokenType.CloseBrace));
            BraceTypeToTokenTypeMap.Add(BraceType.Square, new Tuple<RTokenType, RTokenType>(RTokenType.OpenSquareBracket, RTokenType.CloseSquareBracket));
        }

        public RBraceMatcher(ITextView textView, ITextBuffer textBuffer) : 
            base(textView, textBuffer, new TokenComparer()) {
        }

        protected override IReadOnlyTextRangeCollection<RToken> GetTokens(int start, int length) {
            RTokenizer tokenizer = new RTokenizer();
            ITextProvider tp = new TextProvider(TextBuffer.CurrentSnapshot);
            return tokenizer.Tokenize(tp, 0, tp.Length);
        }

        class TokenComparer : IComparer<RTokenType> {
            public int Compare(RTokenType x, RTokenType y) {
                return x.CompareTo(y);
            }
        }
    }
}

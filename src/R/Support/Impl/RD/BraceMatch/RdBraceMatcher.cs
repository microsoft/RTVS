using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.BraceMatch;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Support.RD.Tokens;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Support.RD.BraceMatch {
    internal sealed class RdBraceMatcher : BraceMatcher<RdToken, RdTokenType> {
        static RdBraceMatcher() {
            BraceTypeToTokenTypeMap.Add(BraceType.Curly, new BraceTokenPair<RdTokenType>(RdTokenType.OpenCurlyBrace, RdTokenType.CloseCurlyBrace));
            BraceTypeToTokenTypeMap.Add(BraceType.Square, new BraceTokenPair<RdTokenType>(RdTokenType.OpenSquareBracket, RdTokenType.CloseSquareBracket));
        }

        public RdBraceMatcher(ITextView textView, ITextBuffer textBuffer) :
            base(textView, textBuffer, new TokenComparer()) {
        }

        protected override IReadOnlyTextRangeCollection<RdToken> GetTokens(int start, int length) {
            RdTokenizer tokenizer = new RdTokenizer();
            ITextProvider tp = new TextProvider(TextBuffer.CurrentSnapshot);
            return tokenizer.Tokenize(tp, 0, tp.Length);
        }

        class TokenComparer : IComparer<RdTokenType> {
            public int Compare(RdTokenType x, RdTokenType y) {
                return x.CompareTo(y);
            }
        }
    }
}

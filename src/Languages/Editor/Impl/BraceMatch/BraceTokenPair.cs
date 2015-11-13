using System;

namespace Microsoft.Languages.Editor.BraceMatch {
    public sealed class BraceTokenPair<TokenTypeT> : Tuple<TokenTypeT, TokenTypeT> {

        public TokenTypeT OpenBrace => this.Item1;
        public TokenTypeT CloseBrace => this.Item2;

        public BraceTokenPair(TokenTypeT openBrace, TokenTypeT closeBrace) :
            base(openBrace, closeBrace) {
        }
    }
}

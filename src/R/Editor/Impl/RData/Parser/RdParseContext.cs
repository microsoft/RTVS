// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Editor.RData.Tokens;

namespace Microsoft.R.Editor.RData.Parser {
    public sealed class RdParseContext {
        public ITextProvider TextProvider { get; private set; }

        public TokenStream<RdToken> Tokens { get; private set; }

        public RdParseContext(IReadOnlyTextRangeCollection<RdToken> tokens, ITextProvider textProvider) {
            this.TextProvider = textProvider;
            this.Tokens = new TokenStream<RdToken>(tokens, RdToken.EndOfStreamToken);
        }

        public bool IsAtKeywordWithParameters(string keyword) {
            return Tokens.CurrentToken.IsKeywordText(TextProvider, keyword) &&
                   Tokens.NextToken.TokenType == RdTokenType.OpenCurlyBrace;
        }
        public bool IsAtKeywordWithParameters() {
            return Tokens.CurrentToken.TokenType == RdTokenType.Keyword &&
                   Tokens.NextToken.TokenType == RdTokenType.OpenCurlyBrace;
        }

        public bool IsAtKeyword(string keyword) {
            return Tokens.CurrentToken.IsKeywordText(TextProvider, keyword);
        }
        public bool IsAtKeyword() {
            return Tokens.CurrentToken.TokenType == RdTokenType.Keyword;
        }
    }
}

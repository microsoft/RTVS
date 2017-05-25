// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Editor.RData.Tokens;

namespace Microsoft.R.Editor.RData.Parser {
    public sealed class RdParseContext {
        /// <summary>
        /// Package name of the function data being parsed
        /// </summary>
        public string PackageName { get; }

        public ITextProvider TextProvider { get; }

        public TokenStream<RdToken> Tokens { get; }

        public RdParseContext(string packageName, IReadOnlyTextRangeCollection<RdToken> tokens, ITextProvider textProvider) {
            PackageName = packageName;
            TextProvider = textProvider;
            Tokens = new TokenStream<RdToken>(tokens, RdToken.EndOfStreamToken);
        }

        public bool IsAtKeywordWithParameters(string keyword) 
            => Tokens.CurrentToken.IsKeywordText(TextProvider, keyword) &&
               Tokens.NextToken.TokenType == RdTokenType.OpenCurlyBrace;

        public bool IsAtKeywordWithParameters() 
            => Tokens.CurrentToken.TokenType == RdTokenType.Keyword &&
               Tokens.NextToken.TokenType == RdTokenType.OpenCurlyBrace;

        public bool IsAtKeyword(string keyword) => Tokens.CurrentToken.IsKeywordText(TextProvider, keyword);
        public bool IsAtKeyword() => Tokens.CurrentToken.TokenType == RdTokenType.Keyword;
    }
}

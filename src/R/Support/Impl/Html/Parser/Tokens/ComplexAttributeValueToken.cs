// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.Html.Core.Parser.Tokens {
    public class ComplexAttributeValueToken : AttributeValueToken {
        private readonly HtmlTokenType _tokenType;
        private readonly char _openQuote;
        private readonly char _closeQuote;

        public ComplexAttributeValueToken(IHtmlToken token, char openQuote, char closeQuote)
            : base(token) {
            _tokenType = token.TokenType;
            _openQuote = openQuote;
            _closeQuote = closeQuote;
        }

        public override HtmlTokenType TokenType => _tokenType;
        public override char OpenQuote  => _openQuote;
        public override char CloseQuote  => _closeQuote;
    }
}

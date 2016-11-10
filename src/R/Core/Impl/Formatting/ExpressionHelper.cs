// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.Formatting {
    /// <summary>
    /// Provides services for expression formatting and indentation
    /// </summary>
    internal sealed class ExpressionHelper {
        private readonly TokenStream<RToken> _tokens;
        private readonly ITextProvider _textProvider;

        public ExpressionHelper(TokenStream<RToken> tokens, ITextProvider textProvider) {
            _tokens = tokens;
            _textProvider = textProvider;
        }

        public bool IsCompleteExpression(int currentTokenIndex) {
            // Within the current scope find if text between scope start and the current
            // token position is a complete expression. We preserve user indentation
            // in multiline expressions so we need to know if a particular position
            // in a middle of an expression. Simple cases liike when previous token was
            // an operator are handled directly. In more complex cases such scope-less
            // function definitions we need to parse the statement.

            int startIndex = 0;
            for (int i = currentTokenIndex - 1; i >= 0; i--) {
                if (_tokens[i].TokenType == RTokenType.OpenCurlyBrace || _tokens[i].TokenType == RTokenType.CloseCurlyBrace) {
                    startIndex = i + 1;
                    break;
                }
            }

            if (startIndex < currentTokenIndex) {
                var startToken = _tokens[startIndex];
                var currentToken = _tokens[currentTokenIndex];

                // Limit token stream since parser may not necessarily stop at the supplied text range end.
                var list = new List<RToken>();
                var tokens = _tokens.Skip(startIndex).Take(currentTokenIndex - startIndex);

                var ts = new TokenStream<RToken>(new TextRangeCollection<RToken>(tokens), RToken.EndOfStreamToken);
                var end = currentToken.TokenType != RTokenType.EndOfStream ? currentToken.Start : _textProvider.Length;

                var ast = RParser.Parse(_textProvider,
                                        TextRange.FromBounds(startToken.Start, end),
                                        ts, new List<RToken>(), null);
                return ast.IsCompleteExpression();
            }
            return true;
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Arguments {
    /// <summary>
    /// Arguments of a function or to indexer.
    /// Does not include outer braces. Each argument is
    /// an expression. Allows for missing arguments. 
    /// Examples are 'a, b[3], c+2' or '1,,3,...'
    /// </summary>
    [DebuggerDisplay("Arguments: {Count} [{Start}...{End})")]
    public sealed class ArgumentList : CommaSeparatedList {
        public ArgumentList(RTokenType terminatingTokenType) :
            base(terminatingTokenType) {
        }

        protected override CommaSeparatedItem CreateItem(IAstNode parent, ParseContext context) {
            RToken currentToken = context.Tokens.CurrentToken;
            RToken nextToken = context.Tokens.NextToken;

            switch (currentToken.TokenType) {
                case RTokenType.Ellipsis:
                    return new EllipsisArgument();

                case RTokenType.Comma:
                    return new MissingArgument();

                case RTokenType.Identifier:
                case RTokenType.String:
                    if (nextToken.TokenType == RTokenType.Operator && context.TextProvider.GetText(nextToken) == "=") {
                        return new NamedArgument();
                    }
                    break;

                case RTokenType.Logical:
                case RTokenType.Complex:
                case RTokenType.NaN:
                case RTokenType.Null:
                case RTokenType.Number:
                case RTokenType.Infinity:
                    if (nextToken.TokenType == RTokenType.Operator && context.TextProvider.GetText(nextToken) == "=") {
                        context.AddError(new ParseError(ParseErrorType.IndentifierExpected, ErrorLocation.Token, currentToken));
                        return new ErrorArgument(CollectErrorArgumentTokens(context));
                    }
                    break;

                case RTokenType.CloseBrace:
                    return null; // no arguments supplied
            }

            return new ExpressionArgument();
        }

        private IEnumerable<RToken> CollectErrorArgumentTokens(ParseContext context) {
            var tokens = new List<RToken>();
            while(!context.Tokens.IsEndOfStream()) {
                if(context.Tokens.CurrentToken.TokenType == RTokenType.Comma ||
                    context.Tokens.CurrentToken.TokenType == RTokenType.CloseBrace ||
                    context.Tokens.CurrentToken.TokenType == RTokenType.CloseSquareBracket ||
                    context.Tokens.CurrentToken.TokenType == RTokenType.CloseDoubleSquareBracket ||
                    context.Tokens.CurrentToken.TokenType == RTokenType.OpenCurlyBrace ||
                    context.Tokens.CurrentToken.TokenType == RTokenType.CloseCurlyBrace) {
                    break;
                }
                tokens.Add(context.Tokens.CurrentToken);
                context.Tokens.MoveToNextToken();
            }
            return tokens;
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.Parser {
    public sealed partial class RParser {
        public static TokenNode ParseKeyword(ParseContext context, IAstNode parent) {
            TokenStream<RToken> tokens = context.Tokens;

            Debug.Assert(tokens.CurrentToken.TokenType == RTokenType.Keyword);

            TokenNode keyword = new TokenNode();
            keyword.Parse(context, parent);

            return keyword;
        }

        public static TokenNode ParseToken(ParseContext context, IAstNode parent) {
            TokenStream<RToken> tokens = context.Tokens;
            TokenNode node = new TokenNode();

            node.Parse(context, parent);
            return node;
        }

        public static TokenNode ParseOpenBraceSequence(ParseContext context, IAstNode parent) {
            TokenStream<RToken> tokens = context.Tokens;

            if (tokens.CurrentToken.TokenType == RTokenType.OpenBrace) {
                TokenNode openBrace = new TokenNode();
                openBrace.Parse(context, parent);

                return openBrace;
            } else {
                context.AddError(new MissingItemParseError(ParseErrorType.OpenBraceExpected, tokens.PreviousToken));
            }

            return null;
        }

        public static TokenNode ParseCloseBraceSequence(ParseContext context, IAstNode parent) {
            TokenStream<RToken> tokens = context.Tokens;

            if (tokens.CurrentToken.TokenType == RTokenType.CloseBrace) {
                return RParser.ParseToken(context, parent);
            }

            context.AddError(new MissingItemParseError(ParseErrorType.CloseBraceExpected, tokens.PreviousToken));
            return null;
        }

        public static IScope ParseScope(ParseContext context, IAstNode parent, bool allowsSimpleScope, string terminatingKeyword) {
            TokenStream<RToken> tokens = context.Tokens;
            IScope scope;

            if (tokens.CurrentToken.TokenType == RTokenType.OpenCurlyBrace) {
                scope = new Scope(string.Empty);
                if (scope.Parse(context, parent)) {
                    return scope;
                }
            } else if (allowsSimpleScope) {
                // Try simple on-line scope as in 'for(...) statement # comment'
                scope = new SimpleScope(terminatingKeyword);
                if (scope.Parse(context, parent)) {
                    return scope;
                }
            } else {
                context.AddError(new MissingItemParseError(ParseErrorType.OpenCurlyBraceExpected, tokens.PreviousToken));
            }

            return null;
        }

        public static RTokenType GetTerminatingTokenType(RTokenType openingTokenType) {
            switch (openingTokenType) {
                case RTokenType.OpenSquareBracket:
                    return RTokenType.CloseSquareBracket;

                case RTokenType.OpenDoubleSquareBracket:
                    return RTokenType.CloseDoubleSquareBracket;

                case RTokenType.OpenCurlyBrace:
                    return RTokenType.CloseCurlyBrace;

                case RTokenType.OpenBrace:
                    return RTokenType.CloseBrace;

                default:
                    Debug.Assert(false, "Unable to determine closing token type");
                    break;
            }

            return RTokenType.Unknown;
        }

        public static RTokenType GetOpeningTokenType(RTokenType closingTokenType) {
            switch (closingTokenType) {
                case RTokenType.CloseBrace:
                    return RTokenType.OpenBrace;

                case RTokenType.CloseSquareBracket:
                    return RTokenType.OpenSquareBracket;

                case RTokenType.CloseDoubleSquareBracket:
                    return RTokenType.OpenDoubleSquareBracket;

                case RTokenType.CloseCurlyBrace:
                    return RTokenType.OpenCurlyBrace;
            }

            return RTokenType.Unknown;
        }

        public static bool IsListTerminator(ParseContext context, RTokenType openingTokenType, RToken token) {
            RTokenType tokenType = token.TokenType;

            if (tokenType == RTokenType.CloseCurlyBrace ||
                tokenType == RTokenType.Semicolon) {
                return true;
            }

            switch (openingTokenType) {
                case RTokenType.OpenBrace:
                    if (tokenType == RTokenType.CloseSquareBracket || tokenType == RTokenType.CloseDoubleSquareBracket) {
                        return true;
                    }
                    break;

                case RTokenType.OpenSquareBracket:
                    if (tokenType == RTokenType.CloseBrace || tokenType == RTokenType.CloseDoubleSquareBracket) {
                        return true;
                    }
                    break;

                case RTokenType.OpenDoubleSquareBracket:
                    if (tokenType == RTokenType.CloseBrace || tokenType == RTokenType.CloseSquareBracket) {
                        return true;
                    }
                    break;
            }

            if (tokenType == RTokenType.Operator && token.IsKeywordText(context.TextProvider, "<-")) {
                return true;
            }

            if (tokenType == RTokenType.Keyword &&
                !token.IsKeywordText(context.TextProvider, "if") &&
                !token.IsKeywordText(context.TextProvider, "function")) {
                return true;
            }

            return false;
        }
    }
}

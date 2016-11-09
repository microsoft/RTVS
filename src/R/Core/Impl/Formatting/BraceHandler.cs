// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.Formatting {
    internal sealed class BraceHandler {
        private readonly Dictionary<RToken, int> _bracetoKeywordMap = new Dictionary<RToken, int>();
        private Stack<RToken> _openBraces = new Stack<RToken>();
        private readonly TokenStream<RToken> _tokens;

        public RToken Top => _openBraces.Count > 0 ? _openBraces.Peek() : null;

        public BraceHandler(TokenStream<RToken> tokens) {
            _tokens = tokens;
        }

        public void HandleBrace() {
            var currentToken = _tokens.CurrentToken;

            switch (currentToken.TokenType) {
                case RTokenType.OpenBrace:
                    if (_tokens.PreviousToken.TokenType == RTokenType.Keyword) {
                        _bracetoKeywordMap[currentToken] = _tokens.Position - 1;
                    }
                    _openBraces.Push(_tokens.CurrentToken);
                    return;

                case RTokenType.OpenCurlyBrace:
                case RTokenType.OpenSquareBracket:
                case RTokenType.OpenDoubleSquareBracket:
                    _openBraces.Push(_tokens.CurrentToken);
                    return;
            }

            if (_openBraces.Count > 0) {
                switch (_tokens.CurrentToken.TokenType) {
                    case RTokenType.CloseBrace:
                    case RTokenType.CloseSquareBracket:
                    case RTokenType.CloseDoubleSquareBracket:
                        if (_openBraces.Peek().TokenType == GetMatchingBraceToken(_tokens.CurrentToken.TokenType)) {
                            _openBraces.Pop();
                        }
                        break;

                    case RTokenType.CloseCurlyBrace:
                        // Close all braces until the nearest curly
                        while (_openBraces.Count > 0) {
                            if (_openBraces.Peek().TokenType == RTokenType.OpenCurlyBrace) {
                                break;
                            }
                            _openBraces.Pop();
                        }

                        if (_openBraces.Count > 0) {
                            _openBraces.Pop();
                        }
                        break;
                }
            }
        }

        public static RTokenType GetMatchingBraceToken(RTokenType tokenType) {
            switch (tokenType) {
                case RTokenType.OpenBrace:
                    return RTokenType.CloseBrace;

                case RTokenType.CloseBrace:
                    return RTokenType.OpenBrace;

                case RTokenType.OpenSquareBracket:
                    return RTokenType.CloseSquareBracket;

                case RTokenType.CloseSquareBracket:
                    return RTokenType.OpenSquareBracket;

                case RTokenType.OpenDoubleSquareBracket:
                    return RTokenType.CloseDoubleSquareBracket;

                case RTokenType.CloseDoubleSquareBracket:
                    return RTokenType.OpenDoubleSquareBracket;
            }

            Debug.Assert(false, "Unknown brace token");
            return RTokenType.Unknown;
        }

        /// <summary>
        /// Tells if scope is opening inside function
        /// or indexer arguments and hence user indentation
        /// of curly braces must be respected.
        /// </summary>
        /// <returns></returns>
        public bool IsInArguments() {
            if (_openBraces.Count > 0) {
                switch (_openBraces.Peek().TokenType) {
                    case RTokenType.OpenBrace:
                    case RTokenType.OpenSquareBracket:
                    case RTokenType.OpenDoubleSquareBracket:
                        return true;
                }
            }
            return false;
        }

        public string GetCloseCurlyBraceIndent(RToken token, TextBuilder tb, RFormatOptions options) {
            Debug.Assert(token.TokenType == RTokenType.CloseCurlyBrace);

            int keywordIndex = -1;
            if (_bracetoKeywordMap.TryGetValue(token, out keywordIndex)) {
                var keywordToken = _tokens[keywordIndex];
                var lineIndentSize = IndentBuilder.GetLineIndentSize(tb, keywordToken.Start, options.TabSize);
                return IndentBuilder.GetIndentString(lineIndentSize, options.IndentType, options.TabSize);
            }
            return string.Empty;
        }

        private int GetLineIndentSize(TextBuilder tb, int position, int tabSize) {
            for (int i = position - 1; i >= 0; i--) {
                if (CharExtensions.IsLineBreak(tb.Text[i])) {
                    return IndentBuilder.TextIndentInSpaces(tb.Text.Substring(i + 1), tabSize);
                }
            }
            return 0;
        }
    }
}

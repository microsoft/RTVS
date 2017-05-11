// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Tokens;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.Formatting {
    /// <summary>
    /// Provides set of services for brace handling in formatting
    /// </summary>
    internal sealed class BraceHandler {
        private readonly Dictionary<RToken, int> _bracetoKeywordPositionMap = new Dictionary<RToken, int>();
        private Stack<RToken> _openBraces = new Stack<RToken>();
        private readonly TokenStream<RToken> _tokens;
        private readonly TextBuilder _tb;

        public RToken Top => _openBraces.Count > 0 ? _openBraces.Peek() : null;

        public BraceHandler(TokenStream<RToken> tokens, TextBuilder tb) {
            _tokens = tokens;
            _tb = tb;
        }

        public void HandleBrace() {
            var currentToken = _tokens.CurrentToken;

            switch (currentToken.TokenType) {
                case RTokenType.OpenBrace:
                    if (_tokens.PreviousToken.TokenType == RTokenType.Keyword) {
                        AssociateKeywordPositionWithOpenBrace(currentToken, GetNearestNonWhitespaceIndex());
                    }
                    _openBraces.Push(_tokens.CurrentToken);
                    return;

                case RTokenType.OpenCurlyBrace:
                    if (_tokens.PreviousToken.TokenType == RTokenType.CloseBrace) {
                        AssociateKeywordPositionWithToken(currentToken, _tokens.PreviousToken);
                    }
                    _openBraces.Push(_tokens.CurrentToken);
                    return;

                case RTokenType.OpenSquareBracket:
                case RTokenType.OpenDoubleSquareBracket:
                    _openBraces.Push(_tokens.CurrentToken);
                    return;
            }

            if (_openBraces.Count > 0) {
                switch (currentToken.TokenType) {
                    case RTokenType.CloseBrace:
                        var openBrace = TryPopMatchingBrace(currentToken);
                        if (openBrace != null) {
                            if (_tokens.NextToken.TokenType == RTokenType.OpenCurlyBrace) {
                                AssociateKeywordPositionWithToken(openBrace, _tokens.NextToken);
                            }
                        }
                        break;

                    case RTokenType.CloseSquareBracket:
                    case RTokenType.CloseDoubleSquareBracket:
                        TryPopMatchingBrace(currentToken);
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

        /// <summary>
        /// Given opening curly brace tries to find keyword that is associated
        /// with the scope and calculate indentation based on the keyword line.
        /// </summary>
        public int GetOpenCurlyBraceIndentSize(RToken openCurlyBraceToken, TextBuilder tb, RFormatOptions options) {
            Debug.Assert(openCurlyBraceToken.TokenType == RTokenType.OpenCurlyBrace);

            int keywordPosition = -1;
            if (_bracetoKeywordPositionMap.TryGetValue(openCurlyBraceToken, out keywordPosition)) {
                return IndentBuilder.GetLineIndentSize(tb, keywordPosition, options.TabSize);
            }
            return 0;
        }

        /// <summary>
        /// Given closing curly brace tries to find keyword that is associated
        /// with the scope and calculate indentation based on the keyword line.
        /// </summary>
        public int GetCloseCurlyBraceIndentSize(RToken closeCurlyBraceToken, TextBuilder tb, RFormatOptions options) {
            Debug.Assert(closeCurlyBraceToken.TokenType == RTokenType.CloseCurlyBrace);

            // Search stack for the first matching brace. Stack enumerates from the top down.
            var openCurlyBraceToken = _openBraces.FirstOrDefault(t => t.TokenType == RTokenType.OpenCurlyBrace);
            if (openCurlyBraceToken != null) {
                return GetOpenCurlyBraceIndentSize(openCurlyBraceToken, tb, options);
            }
            return 0;
        }

        /// <summary>
        /// Associates keyword with the open brace. Used when formatter needs to determine
        /// indentation level of the new formatting scope when it encounters { token.
        /// </summary>
        /// <remarks>
        /// Closing curly indentation is defined by the line that either holds the opening curly brace
        /// or the line that holds keyword that defines the expression that the curly belongs to.
        /// Examples: 
        ///
        ///      x &lt;- 
        ///          function(a) {
        ///          }
        ///
        ///      x &lt;- function(a) {
        ///      }
        ///
        /// First keyword is associated with the open brace, then, when brace pair closes, association
        /// is propagated to the closing brace and then to the opening curly. When curly pair closes
        /// formatter then finds appropriate indentation based on the line that contains the keyword token.
        /// </remarks>

        private void AssociateKeywordPositionWithOpenBrace(RToken openBrace, int keywordPosition) {
            _bracetoKeywordPositionMap[openBrace] = keywordPosition;
        }

        /// <summary>
        /// Propagates keyword association to the target token.
        /// <seealso cref="AssociateKeywordPositionWithOpenBrace"/>
        /// </summary>
        private void AssociateKeywordPositionWithToken(RToken source, RToken target) {
            int keywordPosition;
            if (_bracetoKeywordPositionMap.TryGetValue(source, out keywordPosition)) {
                _bracetoKeywordPositionMap[target] = keywordPosition;
            }
        }

        private RToken TryPopMatchingBrace(RToken token) {
            if (_openBraces.Peek().TokenType == GetMatchingBraceToken(token.TokenType)) {
                return _openBraces.Pop();
            }
            return null;
        }

        private int GetNearestNonWhitespaceIndex() {
            for (int i = _tb.Length - 1; i >= 0; i--) {
                if (!Char.IsWhiteSpace(_tb.Text[i])) {
                    return i;
                }
            }
            return 0;
        }
    }
}

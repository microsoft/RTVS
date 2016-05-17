// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Core.Tokens {
    [DebuggerDisplay("Count={Count}")]
    public sealed class TokenBraceCounter<T> where T : ITextRange {
        T _openBrace1;
        T _closeBrace1;
        T _openBrace2;
        T _closeBrace2;

        private Stack<T> _braces1 = new Stack<T>();
        private Stack<T> _braces2;
        private IComparer<T> _comparer;

        public TokenBraceCounter(T openCurlyBrace, T closeCurlyBrace, IComparer<T> comparer) :
            this(openCurlyBrace, closeCurlyBrace, default(T), default(T), comparer) {
        }

        public TokenBraceCounter(T openBrace1, T closeBrace1, T openBrace2, T closeBrace2, IComparer<T> comparer) {
            _openBrace1 = openBrace1;
            _closeBrace1 = closeBrace1;
            _comparer = comparer;

            if (openBrace2 != null) {
                _braces2 = new Stack<T>();
                _openBrace2 = openBrace2;
                _closeBrace2 = closeBrace2;
            }
        }

        public int Count {
            get { return _braces1.Count; }
        }

        public bool CountBrace(T brace) {
            if (0 == _comparer.Compare(brace, _openBrace1)) {
                _braces1.Push(brace);
            } else if (_braces2 != null && 0 == _comparer.Compare(brace, _openBrace2)) {
                _braces2.Push(brace);
            } else if (0 == _comparer.Compare(brace, _closeBrace1)) {
                if (_braces1.Count > 0) {
                    _braces1.Pop();
                }
            } else if (_braces2 != null && 0 == _comparer.Compare(brace, _closeBrace2)) {
                if (_braces2.Count > 0) {
                    _braces2.Pop();
                }
            } else {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Given token stream and token representing opening brace
        /// finds index of the closing brace token.
        /// </summary>
        public static int GetMatchingBrace(TokenStream<T> tokens, T openBraceToken, T closeBraceToken, IComparer<T> comparer) {
            TokenBraceCounter<T> braceCounter = new TokenBraceCounter<T>(openBraceToken, closeBraceToken, comparer);

            int start = tokens.Position;
            int position = -1;

            while (!tokens.IsEndOfStream()) {
                braceCounter.CountBrace(tokens.CurrentToken);
                if (braceCounter.Count == 0) {
                    position = tokens.Position;
                    break;
                }

                tokens.MoveToNextToken();
            }

            tokens.Position = start;
            return position;
        }

        /// <summary>
        /// Given token stream and token representing opening brace
        /// finds index of the closing brace token.
        /// </summary>
        public int GetMatchingBrace(TokenStream<T> tokens) {
            return GetMatchingBrace(tokens, _openBrace1, _closeBrace1, _comparer);
        }
    }
}

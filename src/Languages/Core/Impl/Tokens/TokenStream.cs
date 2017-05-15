// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Core.Tokens {
    /// <summary>
    /// Generic token stream. Allows fetching tokens safely,
    /// returns special end of stream tokens even before start 
    /// or beyond end of stream. Allows looking beyound end of
    /// the stream and generally helps avoiding exceptions
    /// from out of bound operations.
    /// </summary>
    /// <typeparam name="T">Type of token. Tokens must implement ITextRange.</typeparam>
    public sealed class TokenStream<T> : IEnumerable<T> where T : ITextRange {
        private readonly IReadOnlyTextRangeCollection<T> _tokens;
        private readonly T _endOfStreamToken;
        private int _index;
        private bool _isEndOfStream;

        public TokenStream(IReadOnlyTextRangeCollection<T> tokens, T endOfStreamToken) {
            Check.ArgumentNull(nameof(tokens), tokens);

            _index = 0;
            _tokens = tokens;
            _endOfStreamToken = endOfStreamToken;
            _isEndOfStream = tokens.Length == 0;
            CurrentToken = _isEndOfStream ? _endOfStreamToken : _tokens[0];
        }

        /// <summary>
        /// Number of tokens in the stream
        /// </summary>
        public int Length  => _tokens.Count;

        /// <summary>
        /// Gets or sets position (index of the current token) in the stream.
        /// It is permitted to pass position beyound stream boundaries.
        /// Passing position before the end of stream sets index to the
        /// 'end of stream' token while passing negative position sets
        /// position to -1.
        /// </summary>
        public int Position {
            get { return _index; }
            set {
                _index = value;
                CheckBounds();
            }
        }

        private void CheckBounds() {
            if (_index < 0) {
                _index = 0;
            } else if (_index >= _tokens.Count) {
                _index = _tokens.Count;
            }

            _isEndOfStream = _index >= _tokens.Count;
            CurrentToken = _isEndOfStream ? _endOfStreamToken : _tokens[_index];
        }

        /// <summary>
        /// Returns current token or end of stream token
        /// if current position is at the end of the stream
        /// or before the beginning of the stream.
        /// </summary>
        public T CurrentToken { get; private set; }

        /// <summary>
        /// Next available token or end of stream token if none.
        /// </summary>
        public T NextToken  => LookAhead(1);

        /// <summary>
        /// Previous token or end of stream token if no previous token exists.
        /// </summary>
        public T PreviousToken  => LookAhead(-1);

        /// <summary>
        /// Token 'count' tokens ahead or end of stream token
        /// if position is beyond the token stream end.
        /// </summary>
        /// <param name="count">Nunber of tokens to look ahead</param>
        /// <returns></returns>
        public T LookAhead(int count) => GetTokenAt(_index + count);

        /// <summary>
        /// Token at a specific position or end of stream token
        /// if position is out of stream boundaries.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public T GetTokenAt(int position) {
            if (position >= 0 && position < _tokens.Count) {
                return _tokens[position];
            }

            return _endOfStreamToken;
        }

        public T this[int index] => GetTokenAt(index);
        public bool IsEndOfStream() => _isEndOfStream;

        /// <summary>
        /// Advances stream position to the next token.
        /// Does nothing if position is at the end 
        /// of the stream.
        /// </summary>
        /// <returns>Token that is current after advance 
        /// or end of stream token if position becomes
        /// beyond the end of the stream</returns>
        public T MoveToNextToken() {
            if (_index < _tokens.Count - 1) {
                _index++;
                CurrentToken = _tokens[_index];
                return CurrentToken;
            }

            return Advance(1);
        }

        /// <summary>
        /// Advances stream position by the specified number.
        /// of tokens. Does nothing if position is at the end 
        /// of the stream. Advances to the end of the stream
        /// if passed count is partially within the stream 
        /// boundaries. If advance is negative and goes beyond
        /// the beginning of the stream, sets position to -1.
        /// </summary>
        /// <returns>Token that is current after the advance 
        /// or end of stream token if position becomes
        /// beyond the end of the stream</returns>
        public T Advance(int count) {
            _index += count;
            CheckBounds();

            return CurrentToken;
        }

        /// <summary>
        /// Advances stream position to the nearest token that resides
        /// on the next line (i.e. past the line break) or past the nearest
        /// token as specific by the stop function.
        /// Does nothing if position is at the end of the stream. 
        /// Advances to the end of the stream if current line is 
        /// the last line in the file.
        /// </summary>
        public void MoveToNextLine(ITextProvider textProvider, Func<TokenStream<T>, bool> stopFunction = null) {
            while (!IsEndOfStream()) {
                var currentTokenEnd = CurrentToken.End;
                var nextTokenStart = NextToken.Start;

                MoveToNextToken();

                if (stopFunction != null && stopFunction(this)) {
                    return;
                }

                if (Position < _tokens.Count - 1 &&
                    textProvider.IndexOf("\n", TextRange.FromBounds(currentTokenEnd, nextTokenStart), false) >= 0) {
                    break;
                }
            }
        }

        /// <summary>
        /// Determines if there is a line break between current
        /// and the next token.
        /// </summary>
        public bool IsLineBreakAfter(ITextProvider textProvider, int tokenIndex) {
            if (tokenIndex >= _tokens.Count) {
                return false;
            }

            if (tokenIndex < 0) {
                tokenIndex = 0;
            }

            var currentToken = GetTokenAt(tokenIndex);

            var currentTokenEnd = currentToken.End;
            int nextTokenStart;

            if (tokenIndex < _tokens.Count - 1) {
                var nextToken = _tokens[tokenIndex + 1];
                nextTokenStart = nextToken.Start;
            } else {
                nextTokenStart = textProvider.Length;
            }

            var range = TextRange.FromBounds(currentTokenEnd, nextTokenStart);
            if (textProvider.IndexOf('\n', range) >= 0 || textProvider.IndexOf('\r', range) >= 0) {
                return true;
            }

            return false;
        }

        #region IEnumerable
        public IEnumerator<T> GetEnumerator() => _tokens.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _tokens.GetEnumerator();
        #endregion
    }
}

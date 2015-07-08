using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Core.Tokens
{
    /// <summary>
    /// Generic token stream. Allows fetching tokens safely,
    /// returns special end of stream tokens even before start 
    /// or beyond end of stream. Allows looking beyound end of
    /// the stream and generally helps avoiding exceptions
    /// from out of bound operations.
    /// </summary>
    /// <typeparam name="T">Type of token. Tokens must implement ITextRange.</typeparam>
    public sealed class TokenStream<T> : IEnumerable<T> where T : ITextRange
    {
        private IReadOnlyTextRangeCollection<T> _tokens;
        private int _index;
        private T _endOfStreamToken;

        public TokenStream(IReadOnlyTextRangeCollection<T> tokens, T endOfStreamToken)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException("tokens");
            }

            _index = 0;
            _tokens = tokens;
            _endOfStreamToken = endOfStreamToken;
        }

        /// <summary>
        /// Number of tokens in the stream
        /// </summary>
        public int Length
        {
            get { return _tokens.Count; }
        }

        /// <summary>
        /// Resets stream position to the start.
        /// </summary>
        public void Reset()
        {
            _index = 0;
        }

        /// <summary>
        /// Gets or sets position (index of the current token) in the stream.
        /// It is permitted to pass position beyound stream boundaries.
        /// Passing position before the end of stream sets index to the
        /// 'end of stream' token while passing negative position sets
        /// position to -1.
        /// </summary>
        public int Position
        {
            get { return _index; }
            set
            {
                if (value < 0)
                {
                    _index = 0;
                }
                else if (value >= _tokens.Count)
                {
                    _index = _tokens.Count;
                }
                else
                {
                    _index = value;
                }
            }
        }

        /// <summary>
        /// Returns current token or end of stream token
        /// if current position is at the end of the stream
        /// or before the beginning of the stream.
        /// </summary>
        public T CurrentToken
        {
            get
            {
                if (_index >= 0 && _index < _tokens.Count)
                {
                    return _tokens[_index];
                }

                return _endOfStreamToken;
            }
        }

        /// <summary>
        /// Next available token or end of stream token if none.
        /// </summary>
        public T NextToken
        {
            get
            {
                return LookAhead(1);
            }
        }

        /// <summary>
        /// Previous token or end of stream token if no previous token exists.
        /// </summary>
        public T PreviousToken
        {
            get
            {
                return LookAhead(-1);
            }
        }

        /// <summary>
        /// Token 'count' tokens ahead or end of stream token
        /// if position is beyond the token stream end.
        /// </summary>
        /// <param name="count">Nunber of tokens to look ahead</param>
        /// <returns></returns>
        public T LookAhead(int count)
        {
            return GetTokenAt(_index + count);
        }

        /// <summary>
        /// Token at a specific position or end of stream token
        /// if position is out of stream boundaries.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public T GetTokenAt(int position)
        {
            if (position >= 0 && position < _tokens.Count)
                return _tokens[position];

            return _endOfStreamToken;
        }

        public bool IsEndOfStream()
        {
            return _index >= _tokens.Count;
        }

        /// <summary>
        /// Advances stream position to the next token.
        /// Does nothing if position is at the end 
        /// of the stream.
        /// </summary>
        /// <returns>Token that is current after advance 
        /// or end of stream token if position becomes
        /// beyond the end of the stream</returns>
        public T MoveToNextToken()
        {
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
        public T Advance(int count)
        {
            if (_index + count >= _tokens.Count)
            {
                _index = _tokens.Count;
            }
            else if (_index + count < 0)
            {
                _index = -1;
            }
            else
            {
                _index = _index + count;
            }

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
        public void MoveToNextLine(ITextProvider textProvider, Func<TokenStream<T>, bool> stopFunction = null)
        {
            while (!IsEndOfStream())
            {
                int currentTokenEnd = CurrentToken.End;
                int nextTokenStart = NextToken.Start;

                MoveToNextToken();

                if(stopFunction != null && stopFunction(this))
                {
                    MoveToNextToken();
                    return;
                }

                if (Position < _tokens.Count - 1 && 
                    textProvider.IndexOf("\n", TextRange.FromBounds(currentTokenEnd, nextTokenStart), false) >= 0)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Determines if there is a line break between current
        /// and the next token.
        /// </summary>
        public bool IsLineBreakAfter(ITextProvider textProvider, int tokenIndex)
        {
            if(tokenIndex >= _tokens.Count)
            {
                return false;
            }

            if(tokenIndex < 0)
            {
                tokenIndex = 0;
            }

            T currentToken = GetTokenAt(tokenIndex);

            int currentTokenEnd = currentToken.End;
            int nextTokenStart;

            if (tokenIndex < _tokens.Count - 1)
            {
                T nextToken = GetTokenAt(tokenIndex + 1);
                nextTokenStart = nextToken.Start;
            }
            else
            {
                nextTokenStart = textProvider.Length;
            }

            if (textProvider.IndexOf("\n", TextRange.FromBounds(currentTokenEnd, nextTokenStart), false) >= 0)
            {
                return true;
            }

            return false;
        }

        #region IEnumerable
        public IEnumerator<T> GetEnumerator()
        {
            return _tokens.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _tokens.GetEnumerator();
        }
        #endregion
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Core.Tokens
{
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

        public int Length
        {
            get { return _tokens.Count; }
        }
        public void Reset()
        {
            _index = 0;
        }

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

        public T CurrentToken
        {
            get
            {
                if (_index < _tokens.Count)
                    return _tokens[_index];

                return _endOfStreamToken;
            }
        }

        public T NextToken
        {
            get
            {
                return LookAhead(1);
            }
        }

        public T PreviousToken
        {
            get
            {
                return LookAhead(-1);
            }
        }

        public T LookAhead(int count)
        {
            return GetTokenAt(_index + count);
        }

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

        public T MoveToNextToken()
        {
            return Advance(1);
        }

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

        public void MoveToNextLine(ITextProvider textProvider)
        {
            while (!IsEndOfStream())
            {
                int currentTokenEnd = CurrentToken.End;
                int nextTokenStart = NextToken.End;

                MoveToNextToken();

                if (IsEndOfStream() || Position == _tokens.Count - 1)
                {
                    break;
                }

                if (textProvider.IndexOf("\n", TextRange.FromBounds(currentTokenEnd, nextTokenStart), false) >= 0)
                {
                    break;
                }
            }
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

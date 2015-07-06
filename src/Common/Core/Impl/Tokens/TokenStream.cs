using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Languages.Core.Text;

namespace Microsoft.Languages.Core.Tokens
{
    public sealed class TokenStream<T> : IEnumerable<T> where T : ITextRange
    {
        private IReadOnlyTextRangeCollection<T> tokens;
        private int index;
        private T endOfStreamToken;

        public TokenStream(IReadOnlyTextRangeCollection<T> tokens, T endOfStreamToken)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException("tokens");
            }

            index = 0;
            this.tokens = tokens;
            this.endOfStreamToken = endOfStreamToken;
        }

        public int Length
        {
            get { return this.tokens.Count; }
        }
        public void Reset()
        {
            this.index = 0;
        }

        public int Position
        {
            get { return this.index; }
            set
            {
                if (value < 0)
                {
                    this.index = 0;
                }
                else if (value >= this.tokens.Count)
                {
                    this.index = tokens.Count;
                }
                else
                {
                    this.index = value;
                }
            }
        }

        public T CurrentToken
        {
            get
            {
                if (index < this.tokens.Count)
                    return this.tokens[index];

                return endOfStreamToken;
            }
        }

        public T NextToken
        {
            get
            {
                return this.LookAhead(1);
            }
        }

        public T PreviousToken
        {
            get
            {
                return this.LookAhead(-1);
            }
        }

        public T LookAhead(int count)
        {
            return this.GetTokenAt(index + count);
        }

        public T GetTokenAt(int position)
        {
            if (position >= 0 && position < this.tokens.Count)
                return this.tokens[position];

            return this.endOfStreamToken;
        }

        public bool IsEndOfStream()
        {
            return index >= this.tokens.Count;
        }

        public T MoveToNextToken()
        {
            return this.Advance(1);
        }

        public T Advance(int count)
        {
            if (this.index + count >= this.tokens.Count)
            {
                this.index = this.tokens.Count;
            }
            else if (this.index + count < 0)
            {
                this.index = -1;
            }

            this.index = this.index + count;

            return this.CurrentToken;
        }

        public void MoveToNextLine(ITextProvider textProvider)
        {
            while (!this.IsEndOfStream() && this.Position < this.Length - 1)
            {
                int currentTokenEnd = this.CurrentToken.End;
                int nextTokenStart = this.NextToken.End;

                this.MoveToNextToken();

                if (textProvider.IndexOf("\n", TextRange.FromBounds(currentTokenEnd, nextTokenStart), false) >= 0)
                {
                    break;
                }
            }
        }

        #region IEnumerable
        public IEnumerator<T> GetEnumerator()
        {
            return tokens.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return tokens.GetEnumerator();
        }
        #endregion
    }
}

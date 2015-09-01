using System;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;

namespace Microsoft.R.Core.Tokens
{
    [DebuggerDisplay("[{TokenType} : {Start}...{End}), Length = {Length}")]
    public class RToken : Token<RTokenType>, IComparable<RToken>
    {
        public static RToken EndOfStreamToken = new RToken(RTokenType.EndOfStream, TextRange.EmptyRange);

        public RTokenSubType SubType { get; set; }

        public RToken(RTokenType tokenType)
            : this(tokenType, RTokenSubType.None, TextRange.EmptyRange)
        {
        }

        public RToken(RTokenType tokenType, ITextRange range)
            : this(tokenType, RTokenSubType.None, range)
        {
        }

        public RToken(RTokenType tokenType, RTokenSubType subType, ITextRange range)
            : base(tokenType, range)
        {
            this.SubType = subType;
        }


        public bool IsKeywordText(ITextProvider textProvider, string keywordText)
        {
            if (this.TokenType == RTokenType.Keyword)
            {
                return textProvider.CompareTo(this.Start, this.Length, keywordText, ignoreCase: false);
            }

            return false;
        }

        public int CompareTo(RToken other)
        {
            if (other == null)
                return -1;

            if (this.TokenType == other.TokenType)
                return 0;

            if ((int)this.TokenType < (int)other.TokenType)
                return -1;

            return 1;
        }
    }
}

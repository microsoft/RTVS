using System;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;

namespace Microsoft.R.Support.RD.Tokens
{
    [DebuggerDisplay("[{TokenType} : {Start}...{End}), Length = {Length}")]
    public class RdToken : Token<RdTokenType>, IComparable<RdToken>
    {
        public static RdToken EndOfStreamToken = new RdToken(RdTokenType.EndOfStream);

        public RdToken(RdTokenType tokenType)
            : this(tokenType, TextRange.EmptyRange)
        {
        }

        public RdToken(RdTokenType tokenType, ITextRange range)
            : base(tokenType, range)
        {
        }

        public override bool IsComment
        {
            get
            {
                return this.TokenType == RdTokenType.Comment;
            }
        }

        public override bool IsString
        {
            get
            {
                return this.TokenType == RdTokenType.String;
            }
        }

        public override bool IsKeyword
        {
            get
            {
                return this.TokenType == RdTokenType.Keyword;
            }
        }

        public override bool IsPunctuation
        {
            get
            {
                return this.TokenType == RdTokenType.OpenCurlyBrace ||
                       this.TokenType == RdTokenType.CloseCurlyBrace ||
                       this.TokenType == RdTokenType.OpenSquareBracket ||
                       this.TokenType == RdTokenType.CloseSquareBracket;
            }
        }

        public bool IsKeywordText(ITextProvider textProvider, string keywordText)
        {
            if (this.TokenType == RdTokenType.Keyword)
            {
                return textProvider.CompareTo(this.Start, this.Length, keywordText, ignoreCase: false);
            }

            return false;
        }

        public int CompareTo(RdToken other)
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

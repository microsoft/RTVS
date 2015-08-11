using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;

namespace Microsoft.R.Support.RD.Tokens
{
    [DebuggerDisplay("[{TokenType} : {Start}...{End}), Length = {Length}")]
    public class RdToken : Token<RdTokenType>
    {
        public static RdToken EndOfStreamToken = new RdToken(RdTokenType.EndOfStream, TextRange.EmptyRange);

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
                return this.TokenType == RdTokenType.OpenBrace || this.TokenType == RdTokenType.CloseBrace;
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
    }
}

using System;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;

namespace Microsoft.Markdown.Editor.Tokens
{
    [DebuggerDisplay("[{TokenType} : {Start}...{End}), Length = {Length}")]
    public class MdToken : Token<MdTokenType>, IComparable<MdToken>
    {
        public static MdToken EndOfStreamToken = new MdToken(MdTokenType.EndOfStream);

        public MdToken(MdTokenType tokenType)
            : this(tokenType, TextRange.EmptyRange)
        {
        }

        public MdToken(MdTokenType tokenType, ITextRange range)
            : base(tokenType, range)
        {
        }

        public int CompareTo(MdToken other)
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

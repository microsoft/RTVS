using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Core.Tokens;

namespace Microsoft.R.Core.Tokens
{
    [DebuggerDisplay("[{TokenType} : {Start}...{End}), Length = {Length}")]
    public class RToken : Token<RTokenType>
    {
        public static RToken EndOfStreamToken = new RToken(RTokenType.EndOfStream, TextRange.EmptyRange);

        public RTokenSubType SubType { get; set; }

        public RToken(RTokenType tokenType, ITextRange range)
            : this(tokenType, RTokenSubType.None, range)
        {
        }

        public RToken(RTokenType tokenType, RTokenSubType subType, ITextRange range)
            : base(tokenType, range)
        {
            this.SubType = subType;
        }

        public override bool IsComment
        {
            get
            {
                return this.TokenType == RTokenType.Comment;
            }
        }

        public override bool IsString
        {
            get
            {
                return this.TokenType == RTokenType.String;
            }
        }

        public override bool IsNumber
        {
            get
            {
                return this.TokenType == RTokenType.Number || TokenType == RTokenType.Complex;
            }
        }
        public override bool IsKeyword
        {
            get
            {
                return this.TokenType == RTokenType.Keyword;
            }
        }

        public override bool IsPunctuation
        {
            get
            {
                return this.TokenType == RTokenType.Comma || this.TokenType == RTokenType.Semicolon;
            }
        }

        public override bool IsOperator
        {
            get
            {
                return this.TokenType == RTokenType.Operator;
            }
        }

        public bool IsBuiltin
        {
            get { return this.TokenType == RTokenType.Identifier && this.SubType == RTokenSubType.BuiltinFunction; }
        }
    }
}

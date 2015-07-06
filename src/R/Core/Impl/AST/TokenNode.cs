using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST
{
    /// <summary>
    /// The simplest type of AST tree item representing a single token.
    /// This item does not have children. Examples are identifiers,
    /// constants, operators, etc. All leaf nodes MUST be a token items.
    /// </summary>
    [DebuggerDisplay("[{Token.TokenType} : {Start}...{End}), Length = {Length}]")]
    public class TokenNode : AstNode
    {
        public RToken Token { get; protected set; }

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            RToken currentToken = context.Tokens.CurrentToken;

            this.Token = currentToken;
            context.Tokens.MoveToNextToken();

            return base.Parse(context, parent);
        }

        #region ITextRange
        public override int Start
        {
            get { return this.Token.Start; }
        }

        public override int End
        {
            get { return this.Token.End; }
        }

        public override IReadOnlyTextRangeCollection<IAstNode> Children
        {
            get { return TextRangeCollection<IAstNode>.EmptyCollection; }
        }

        public override bool Contains(int position)
        {
            return this.Token.Contains(position);
        }

        public override void Shift(int offset)
        {
            this.Token.Shift(offset);
        }

        public override void ShiftStartingFrom(int position, int offset)
        {
            if(this.Token.Start < position && position < this.Token.End)
            {
                throw new InvalidOperationException("Cannot shift text range from position within it");
            }

            if (position <= this.Token.Start)
            {
                this.Token.Shift(offset);
            }
        }
        #endregion

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            string name = (this.Root != null) ? 
                this.Root.TextProvider.GetText(this.Token) : this.Token.ToString();

            sb.Append(name);
            sb.Append(" [");
            sb.Append(this.Start);
            sb.Append("...");
            sb.Append(this.End);
            sb.Append(']');

            return sb.ToString();
        }
    }
}

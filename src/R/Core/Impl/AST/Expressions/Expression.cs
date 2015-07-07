using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Expressions
{
    /// <summary>
    /// Represents mathematical expression such as x + 2.0 + y * (z - 1). 
    /// Parsing splits expression into triplets like A + [remaining part].
    /// Expression may include braces and may or may not have second part
    /// or the operator. For example: (x) is valid expression as is single
    /// identifier such as in 'x &lt;- y' assignments.  
    /// </summary>
    public sealed class Expression : RValueNode<RObject>
    {
        private bool braceless;

        public TokenNode OpenBrace { get; private set; }
        public IAstNode Content { get; private set; }
        public TokenNode CloseBrace { get; private set; }

        public Expression(bool braceless = false)
        {
            this.braceless = braceless;
        }

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            if (!braceless && context.Tokens.CurrentToken.TokenType == RTokenType.OpenBrace)
            {
                this.OpenBrace = RParser.ParseToken(context, this);
            }

            ExpressionParser expressionParser = new ExpressionParser();
            this.Content = expressionParser.Parse(context, this);

            if (this.OpenBrace != null)
            {
                if (context.Tokens.CurrentToken.TokenType == RTokenType.CloseBrace)
                {
                    this.CloseBrace = RParser.ParseToken(context, this);
                }
                else
                {
                    context.Errors.Add(new MissingItemParseError(ParseErrorType.CloseBraceExpected, context.Tokens.PreviousToken));
                    return false;
                }
            }

            if (this.Content != null)
            {
                return base.Parse(context, parent);
            }

            return false;
        }

        public override string ToString()
        {
            if (this.Root != null)
            {
                string text = this.Root.TextProvider.GetText(this);
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }

            return "Expression";
        }
    }
}

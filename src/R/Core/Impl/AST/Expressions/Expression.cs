using System.Diagnostics;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Expressions.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Core.AST.Expressions
{
    /// <summary>
    /// Represents mathematical or conditional expression, 
    /// assignment, function or operator definition optionally
    /// enclosed in braces. Expression is a tree and may have
    /// nested extressions in its content.
    /// </summary>
    [DebuggerDisplay("Expression [{Start}...{End})")]
    public sealed class Expression : RValueNode<RObject>, IExpression
    {
        private bool _braceless;
        private string _terminatingKeyword;

        #region IExpression
        public TokenNode OpenBrace { get; private set; }
        public IAstNode Content { get; private set; }
        public TokenNode CloseBrace { get; private set; }
        #endregion

        public Expression(bool braceless = false)
        {
            _braceless = braceless;
        }

        public Expression(bool braceless, string terminatingKeyword): 
            this(braceless)
        {
            _terminatingKeyword = terminatingKeyword;
        }

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            if (!_braceless && context.Tokens.CurrentToken.TokenType == RTokenType.OpenBrace)
            {
                this.OpenBrace = RParser.ParseToken(context, this);
            }

            ExpressionParser expressionParser = new ExpressionParser(_terminatingKeyword);
            this.Content = expressionParser.Parse(context, this);

            if (this.OpenBrace != null)
            {
                if (context.Tokens.CurrentToken.TokenType == RTokenType.CloseBrace)
                {
                    this.CloseBrace = RParser.ParseToken(context, this);
                }
                else
                {
                    context.AddError(new MissingItemParseError(ParseErrorType.CloseBraceExpected, context.Tokens.PreviousToken));
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

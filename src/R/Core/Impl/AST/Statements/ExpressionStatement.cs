using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Expressions;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Core.AST.Statements
{
    /// <summary>
    /// Statement that either defines variable (i.e. first time use)
    /// or assign a new value to the variable or any of its parts.
    /// </summary>
    public sealed class ExpressionStatement : Statement
    {
        public Expression Expression { get; private set; }

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            this.Expression = new Expression(braceless: true);
            if (this.Expression.Parse(context, this))
            {
                if(this.Expression.Children.Count == 1 && this.Expression.Children[0] is Expression)
                {
                    // Promote up
                    this.Expression = this.Expression.Children[0] as Expression;
                    this.Expression.Parent = null;
                    this.children.RemoveAt(0);
                    this.Expression.Parent = this;
                }

                return base.Parse(context, parent);
            }

            return false;
        }

        public override string ToString()
        {
            return this.Expression.ToString() + base.ToString();
        }
    }
}

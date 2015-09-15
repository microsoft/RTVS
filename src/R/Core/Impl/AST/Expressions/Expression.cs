using System.Diagnostics;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Expressions.Definitions;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Core.AST.Expressions
{
    /// <summary>
    /// Represents mathematical or conditional expression, 
    /// assignment, function or operator definition optionally
    /// enclosed in braces. Expression is a tree and may have
    /// nested extressions in its content.
    /// </summary>
    [DebuggerDisplay("Expression [{Start}...{End})")]
    public sealed partial class Expression : RValueNode<RObject>, IExpression
    {
        private string _terminatingKeyword;

        #region IExpression
        public IRValueNode Content { get; internal set; }
        #endregion

        public Expression()
        {
        }

        public Expression(string terminatingKeyword)
        {
            _terminatingKeyword = terminatingKeyword;
        }

        public override bool Parse(ParseContext context, IAstNode parent)
        {
            if (Parse(context) && this.Children.Count > 0)
            {
                return base.Parse(context, parent);
            }

            return false;
        }

        public override RObject GetValue()
        {
            if (Content != null)
            {
                return Content.GetValue();
            }

            return base.GetValue();
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

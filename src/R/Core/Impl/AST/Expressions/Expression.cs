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
        private bool _inGroup;

        #region IExpression
        public IRValueNode Content { get; internal set; }
        #endregion

        /// <summary>
        /// Constructs an expression that will be parsed as it is
        /// inside braces (in a group) so expression parsing
        /// will continue even if there is a line break
        /// that would normally terminate the expression.
        /// </summary>
        /// <param name="inGroup"></param>
        public Expression(bool inGroup = false)
        {
            _inGroup = inGroup;
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

using System.Diagnostics;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Definitions;

namespace Microsoft.R.Core.AST
{
    /// <summary>
    /// Base class for complex nodes representing R-values such as
    /// function calls, expressions and similar constructs.
    /// </summary>
    public abstract class RValueTokenNode<T> : TokenNode, IRValueNode where T : RObject
    {
        protected T nodeValue;

        #region IRValueNode
        public RObject GetValue()
        {
            if(this.nodeValue == null)
            {
                this.nodeValue = (T)this.Root.CodeEvaluator.Evaluate(this);
            }

            return this.nodeValue;
        }
        #endregion
    }
}

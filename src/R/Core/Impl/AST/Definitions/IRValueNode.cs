using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.Parser.Definitions;

namespace Microsoft.R.Core.AST.Definitions
{
    /// <summary>
    /// Represents R-value: the value that can only appear at 
    /// the right side of the expression (or at the left side of
    /// a right-hand assignment such as -&gt;&gt;. Typical example
    /// is a constant (number, string, logical) or a function call.
    /// </summary>
    public interface IRValueNode: IAstNode
    {
        RObject GetValue();
    }
}

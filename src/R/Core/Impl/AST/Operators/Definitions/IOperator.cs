using System.Diagnostics;
using Microsoft.R.Core.AST.Definitions;

namespace Microsoft.R.Core.AST.Operators.Definitions
{
    public interface IOperator: IAstNode, IRValueNode
    {
        OperatorType OperatorType { get; }

        IRValueNode LeftOperand { get; set; }

        IRValueNode RightOperand { get; set; }

        int Precedence { get; }

        bool IsUnary { get; }

        Association Association { get; }
    }
}

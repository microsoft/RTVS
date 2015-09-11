using System.Diagnostics;
using Microsoft.R.Core.AST.DataTypes;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Operators.Definitions;

namespace Microsoft.R.Core.AST.Operators
{
    [DebuggerDisplay("[{OperatorType} [{Start}...{End})]")]
    public abstract class Operator : RValueNode<RObject>, IOperator
    {
        #region IOperator
        public IAstNode LeftOperand { get; set; }

        public virtual OperatorType OperatorType { get; private set; }

        public IAstNode RightOperand { get; set; }

        public virtual int Precedence { get; internal set; }

        public virtual bool IsUnary { get; private set; }

        public virtual Association Association { get; internal set; }
        #endregion

        public static bool IsPossibleUnary(OperatorType operatorType)
        {
            switch (operatorType)
            {
                case OperatorType.Tilde:
                case OperatorType.Not:
                case OperatorType.Add:
                case OperatorType.Subtract:
                    return true;
            }

            return false;
        }
    }
}

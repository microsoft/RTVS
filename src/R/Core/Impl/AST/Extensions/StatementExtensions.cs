using Microsoft.R.Core.AST.Functions;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.AST.Statements;
using Microsoft.R.Core.AST.Variables;

namespace Microsoft.R.Core.AST {
    public static class StatementExtensions {
        /// <summary>
        /// Given expression statement determines if it defines a function
        /// and if so, returns the function definition and the variable
        /// it is assigned to.
        /// </summary>
        public static IFunctionDefinition GetVariableOrFunctionDefinition(this IExpressionStatement es, out IVariable v) {
            v = null;

            // Tree:
            //       <-
            //    x      function(a)
            //
            //
            var c = es?.Expression?.Children;
            if (c?.Count == 1) {
                if (c[0] is IOperator op) {
                    if (op.OperatorType == OperatorType.LeftAssign || op.OperatorType == OperatorType.Equals) {
                        v = op.LeftOperand as Variable;
                        if (v != null) {
                            return op.RightOperand as IFunctionDefinition;
                        }
                    } else if (op.OperatorType == OperatorType.RightAssign) {
                        v = op.RightOperand as Variable;
                    }
                }
            }
            return null;
        }
    }
}

using Microsoft.R.Core.AST.Functions.Definitions;
using Microsoft.R.Core.AST.Operators;
using Microsoft.R.Core.AST.Operators.Definitions;
using Microsoft.R.Core.AST.Statements.Definitions;
using Microsoft.R.Core.AST.Variables;

namespace Microsoft.R.Core.AST {
    public static class StatementExtensions {
        public static IFunctionDefinition GetVariableOrFunctionDefinition(this IExpressionStatement es, out Variable v) {
            IFunctionDefinition fd = null;
            v = null;

            if (es == null || es.Expression == null) {
                return null;
            }

            var c = es.Expression.Children;
            if (c.Count == 1) {
                var op = c[0] as IOperator;
                if (op != null) {
                    if (op.OperatorType == OperatorType.LeftAssign) {
                        v = op.LeftOperand as Variable;
                    } else if (op.OperatorType == OperatorType.RightAssign) {
                        v = op.LeftOperand as Variable;
                    }
                    if (v != null) {
                        fd = op.RightOperand as IFunctionDefinition;
                    }
                }
            }
            return fd;
        }
    }
}

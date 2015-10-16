using Microsoft.R.Core.AST.Definitions;

namespace Microsoft.R.Core.AST.Expressions.Definitions
{
    /// <summary>
    /// Represents expression that is used in enumerations
    /// such as in 'for(x in exp) { }'. Enumerable expressions
    /// do not allow braces and cannot be nested.
    /// </summary>
    public interface IEnumerableExpression: IAstNode
    {
        /// <summary>
        /// Name of variable in 'for(variable_name in ...)'
        /// </summary>
        TokenNode VariableName { get; }

        /// <summary>
        /// Token of the 'in' operator
        /// </summary>
        TokenNode InOperator { get; }

        /// <summary>
        /// Expression in 'for(variable_name in expression)'
        /// </summary>
        IExpression Expression { get; }
    }
}

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
        TokenNode VariableName { get; }
        TokenNode InOperator { get; }
        IExpression Expression { get; }
    }
}

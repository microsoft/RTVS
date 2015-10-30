using Microsoft.R.Core.AST.Expressions.Definitions;

namespace Microsoft.R.Core.AST.Statements.Definitions {
    /// <summary>
    /// Statement that is based on expression. Expression 
    /// can be mathematical, conditional, assignment, function 
    /// or operator definition.
    /// </summary>
    public interface IExpressionStatement : IStatement {
        IExpression Expression { get; }
    }
}

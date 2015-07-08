using Microsoft.R.Core.AST.Expressions.Definitions;

namespace Microsoft.R.Core.AST.Statements.Definitions
{
    /// <summary>
    /// Represents statement that is based on a keyword
    /// and has conditional or enumerable expression
    /// such as while(...) or for(...) with { } scope.
    /// </summary>
    public interface IKeywordExpressionStatement: IKeywordStatement
    {
        TokenNode OpenBrace { get; }
        IExpression Expression { get; }
        TokenNode CloseBrace { get; }
    }
}

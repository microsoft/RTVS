using Microsoft.R.Core.AST.Expressions.Definitions;

namespace Microsoft.R.Core.AST.Statements.Definitions {
    /// <summary>
    /// Represents sequence that consists of a keyword
    /// followed by braces and expression such as in
    /// statements like while(...) or for(...).
    /// </summary>
    public interface IKeywordExpression : IKeyword {
        TokenNode OpenBrace { get; }
        IExpression Expression { get; }
        TokenNode CloseBrace { get; }
    }
}

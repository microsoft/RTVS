using Microsoft.R.Core.AST.Scopes.Definitions;

namespace Microsoft.R.Core.AST.Statements.Definitions {
    /// <summary>
    /// Represents statement that is based on a keyword
    /// and has a scope such as 'repeat { }'.
    /// </summary>
    public interface IKeywordScopeStatement : IKeyword, IStatement {
        IScope Scope { get; }
    }
}

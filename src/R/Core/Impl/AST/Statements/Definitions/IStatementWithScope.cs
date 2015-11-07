using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Scopes.Definitions;

namespace Microsoft.R.Core.AST.Statements.Definitions {
    public interface IAstNodeWithScope : IAstNode {
        IScope Scope { get; }
    }
}

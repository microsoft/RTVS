using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.AST.Statements.Definitions;

namespace Microsoft.R.Core.AST.Functions.Definitions {
    public interface IFunctionDefinition : IFunction, IKeyword, IAstNodeWithScope {
    }
}

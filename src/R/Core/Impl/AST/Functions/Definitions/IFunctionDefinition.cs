using Microsoft.R.Core.AST.Scopes.Definitions;

namespace Microsoft.R.Core.AST.Functions.Definitions
{
    public interface IFunctionDefinition: IFunction
    {
        TokenNode Keyword { get; }

        IScope Scope { get; }
    }
}

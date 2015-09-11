using Microsoft.R.Core.AST.Scopes.Definitions;

namespace Microsoft.R.Core.AST.Functions.Definitions
{
    public interface IFunctionDefinition: IFunction
    {
        /// <summary>
        /// 'function' keyword. Always present.
        /// </summary>
        TokenNode Keyword { get; }

        /// <summary>
        /// Function definition scope. Can be typical
        /// { } scope or a simple scope as in
        /// x &lt;- function(a) return(a+1)
        /// </summary>
        IScope Scope { get; }
    }
}

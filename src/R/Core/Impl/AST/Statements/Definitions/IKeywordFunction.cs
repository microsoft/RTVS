using Microsoft.R.Core.AST.Functions.Definitions;

namespace Microsoft.R.Core.AST.Statements.Definitions {
    /// <summary>
    /// Represents function which name is a keyword like switch()
    /// </summary>
    public interface IKeywordFunction : IKeyword, IFunction {
    }
}

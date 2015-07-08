using Microsoft.R.Core.AST.Definitions;

namespace Microsoft.R.Core.AST.Statements.Definitions
{
    /// <summary>
    /// Represents a statement with optional terminating semicolon.
    /// </summary>
    public interface IStatement: IAstNode
    {
        /// <summary>
        /// Optional terminating semicolon
        /// </summary>
        TokenNode Semicolon { get; }
    }
}

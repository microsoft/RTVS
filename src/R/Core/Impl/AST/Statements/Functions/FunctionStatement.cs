using System.Diagnostics;
using Microsoft.R.Core.AST.Functions;
using Microsoft.R.Core.AST.Statements.Definitions;

namespace Microsoft.R.Core.AST.Statements
{
    /// <summary>
    /// Represents statement that consists of a function.
    /// Typically used in inline if statement scopes
    /// </summary>
    [DebuggerDisplay("[FunctionStatement]")]
    public sealed class FunctionStatement : FunctionDefinition, IStatement, IKeyword
    {
        /// <summary>
        /// Optional terminating semicolon
        /// </summary>
        public TokenNode Semicolon { get; private set; }
    }
}

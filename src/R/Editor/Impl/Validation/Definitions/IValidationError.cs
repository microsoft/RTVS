using System;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Editor.Validation.Definitions
{
    /// <summary>
    /// Represents validation result. May be based
    /// on AST node or a standalone token.
    /// </summary>
    public interface IValidationError: ITextRange
    {
        /// <summary>
        /// Token the error applies to.
        /// </summary>
        RToken Token { get; }

        /// <summary>
        /// AST node the error applies to.
        /// </summary>
        IAstNode Node { get; }

        /// <summary>
        /// Message to place in a task list and/or tooltip
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Location of the error in the element.
        /// </summary>
        ValidationErrorLocation Location { get; }

        /// <summary>
        /// Error severity
        /// </summary>
        ValidationErrorSeverity Severity { get; }
    }
}

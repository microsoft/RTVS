using System;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Editor.Validation.Definitions
{
    /// <summary>
    /// Represents validation result. Due to asynchronous nature of the validation 
    /// the data does not include physical location of the error in the text buffer
    /// or a reference to the node instance. Instread, it includes unique
    /// node key as well as error location as an enumeration. Code that creates
    /// actual squiggles and pushes data to a task list is normally running on UI
    /// thread and maps location enum to the current physical position. 
    /// </summary>
    public interface IValidationError
    {
        /// <summary>
        /// Key of the AST node the error applies to.
        /// </summary>
        RToken Token { get; }

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

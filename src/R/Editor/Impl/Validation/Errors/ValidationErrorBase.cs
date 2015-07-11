using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Validation.Definitions;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Editor.Validation.Errors
{
    [ExcludeFromCodeCoverage]
    public class ValidationErrorBase : IValidationError
    {
        /// <summary>
        /// AST node key in the parse tree
        /// </summary>
        public RToken Token { get; private set; }

        /// <summary>
        /// Error or warning message.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Error location
        /// </summary>
        public ValidationErrorLocation Location { get; private set; }

        /// <summary>
        /// Error severity
        /// </summary>
        public ValidationErrorSeverity Severity { get; private set; }

        public ValidationErrorBase(RToken token, string message, ValidationErrorLocation location, ValidationErrorSeverity severity)
        {
            Token = token;
            Message = message;
            Severity = severity;
            Location = location;
        }
    }
}

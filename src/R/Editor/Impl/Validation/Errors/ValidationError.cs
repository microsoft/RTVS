using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Validation.Definitions;

namespace Microsoft.R.Editor.Validation.Errors
{
    [ExcludeFromCodeCoverage]
    public class ValidationError : ValidationErrorBase
    {
        /// <summary>
        /// Constructs validation error based on existing error.
        /// </summary>
        public ValidationError(IValidationError error) :
            base(error.Token, error.Message, error.Location, error.Severity)
        {
        }

        /// <summary>
        /// Constructs validation error for an element at a specified location.
        /// </summary>
        public ValidationError(RToken token, string message, ValidationErrorLocation location) :
            base(token, message, location, ValidationErrorSeverity.Error)
        {
        }
    }
}

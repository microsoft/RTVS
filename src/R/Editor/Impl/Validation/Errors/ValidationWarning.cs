using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Validation.Definitions;

namespace Microsoft.R.Editor.Validation.Errors
{
    [ExcludeFromCodeCoverage]
    public class ValidationWarning : ValidationErrorBase
    {
        /// <summary>
        /// Constructs validation warning for an element at a specified location.
        /// </summary>
        public ValidationWarning(RToken token, string message, ValidationErrorLocation location) :
            base(token, message, location, ValidationErrorSeverity.Warning)
        {
        }
    }
}

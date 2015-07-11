using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Validation.Definitions;

namespace Microsoft.R.Editor.Validation.Errors
{
    [ExcludeFromCodeCoverage]
    public class ValidationMessage : ValidationErrorBase
    {
        public ValidationMessage(RToken token, string message, ValidationErrorLocation location) :
            base(token, message, location, ValidationErrorSeverity.Informational)
        {
        }
    }
}

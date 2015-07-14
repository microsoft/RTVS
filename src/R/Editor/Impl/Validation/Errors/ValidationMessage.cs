using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Validation.Definitions;

namespace Microsoft.R.Editor.Validation.Errors
{
    [ExcludeFromCodeCoverage]
    public class ValidationMessage : ValidationErrorBase
    {
        public ValidationMessage(IAstNode node, RToken token, string message, ValidationErrorLocation location) :
            base(node, token, message, location, ValidationErrorSeverity.Informational)
        {
        }

        public ValidationMessage(RToken token, string message, ValidationErrorLocation location) :
            base(null, token, message, location, ValidationErrorSeverity.Informational)
        {
        }
    }
}

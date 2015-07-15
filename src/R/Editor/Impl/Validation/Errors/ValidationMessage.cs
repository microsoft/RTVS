using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.R.Editor.Validation.Errors
{
    [ExcludeFromCodeCoverage]
    public class ValidationMessage : ValidationErrorBase
    {
        public ValidationMessage(IAstNode node, RToken token, string message, ErrorLocation location) :
            base(node, token, message, location, ErrorSeverity.Informational)
        {
        }

        public ValidationMessage(RToken token, string message, ErrorLocation location) :
            base(null, token, message, location, ErrorSeverity.Informational)
        {
        }
    }
}

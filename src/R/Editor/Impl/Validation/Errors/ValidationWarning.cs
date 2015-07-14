using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.AST.Definitions;
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
            this(null, token, message, location)
        {
        }
        public ValidationWarning(IAstNode node, RToken token, string message, ValidationErrorLocation location) :
             base(node, token, message, location, ValidationErrorSeverity.Warning)
        {
        }
    }
}

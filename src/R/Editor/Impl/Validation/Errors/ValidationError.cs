using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Validation.Definitions;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Editor.Validation.Errors
{
    [ExcludeFromCodeCoverage]
    public class ValidationError : ValidationErrorBase
    {
        /// <summary>
        /// Constructs validation error based on existing error.
        /// </summary>
        public ValidationError(IValidationError error) :
            base(error.Node, error.Token, error.Message, error.Location, error.Severity)
        {
        }

        /// <summary>
        /// Constructs validation error for an element at a specified location.
        /// </summary>
        public ValidationError(IAstNode node, RToken token, string message, ErrorLocation location) :
            base(node, token, message, location, ErrorSeverity.Error)
        {
        }

        /// <summary>
        /// Constructs validation error for an element at a specified location.
        /// </summary>
        public ValidationError(RToken token, string message, ErrorLocation location) :
            base(null, token, message, location, ErrorSeverity.Error)
        {
        }
    }
}

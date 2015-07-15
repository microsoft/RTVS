using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Parser;
using Microsoft.R.Core.Tokens;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.R.Editor.Validation.Errors
{
    [ExcludeFromCodeCoverage]
    public class ValidationWarning : ValidationErrorBase
    {
        /// <summary>
        /// Constructs validation warning for an element at a specified location.
        /// </summary>
        public ValidationWarning(RToken token, string message, ErrorLocation location) :
            this(null, token, message, location)
        {
        }
        public ValidationWarning(IAstNode node, RToken token, string message, ErrorLocation location) :
             base(node, token, message, location, ErrorSeverity.Warning)
        {
        }
    }
}

using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Validation.Definitions;

namespace Microsoft.R.Editor.Validation.Errors
{
    [ExcludeFromCodeCoverage]
    public class ValidationErrorBase : TextRange, IValidationError
    {
        /// <summary>
        /// AST node in the parse tree
        /// </summary>
        public IAstNode Node { get; private set; }

        /// <summary>
        /// Token that produced the error.
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

        public ValidationErrorBase(IAstNode node, RToken token, string message, ValidationErrorLocation location, ValidationErrorSeverity severity) :
            base(node != null ? node : token as ITextRange)
        {
            Node = node;
            Token = token;

            Message = message;
            Severity = severity;
            Location = location;
        }
    }
}

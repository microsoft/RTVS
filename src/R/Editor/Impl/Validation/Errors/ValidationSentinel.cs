using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Validation.Definitions;

namespace Microsoft.R.Editor.Validation.Errors
{
    [ExcludeFromCodeCoverage]
    public class ValidationSentinel : ValidationErrorBase
    {
        /// <summary>
        /// Constructs 'barrier' pseudo error that clears all messages for a given node.
        /// </summary>
        public ValidationSentinel(RToken token) :
            base(token, String.Empty, ValidationErrorLocation.Node, ValidationErrorSeverity.Error)
        {
        }
    }
}

using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Editor.Validation.Errors
{
    [ExcludeFromCodeCoverage]
    public class ValidationMessage : ValidationErrorBase
    {
        public ValidationMessage(ITextRange range, string message, ErrorLocation location) :
            base(range, message, location, ErrorSeverity.Informational)
        {
        }
    }
}

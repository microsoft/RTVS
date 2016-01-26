using System.Collections.Generic;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.Parser;
using Microsoft.R.Editor.Validation.Definitions;

namespace Microsoft.R.Editor.Validation.Errors
{
    public class ValidationErrorCollection : List<IValidationError>
    {
        public void Add(ITextRange range, string message, ErrorLocation location)
        {
            Add(new ValidationError(range, message, location));
        }
    }
}

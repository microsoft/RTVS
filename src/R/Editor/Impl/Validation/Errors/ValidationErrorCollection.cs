using System.Collections.Generic;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Validation.Definitions;
using Microsoft.R.Core.Parser;

namespace Microsoft.R.Editor.Validation.Errors
{
    public class ValidationErrorCollection : List<IValidationError>
    {
        public void Add(RToken token, string message)
        {
            Add(token, message, ErrorLocation.Token);
        }

        public void Add(IAstNode node, string message)
        {
            Add(node, null, message, ErrorLocation.Token);
        }

        public void Add(RToken token, string message, ErrorLocation location)
        {
            Add(new ValidationError(null, token, message, location));
        }

        public void Add(IAstNode node, RToken token, string message, ErrorLocation location)
        {
            Add(new ValidationError(node, token, message, location));
        }
    }
}

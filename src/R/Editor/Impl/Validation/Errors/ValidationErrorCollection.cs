using System.Collections.Generic;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.Validation.Definitions;

namespace Microsoft.R.Editor.Validation.Errors
{
    public class ValidationErrorCollection : List<IValidationError>
    {
        public void Add(RToken token, string message)
        {
            Add(token, message, ValidationErrorLocation.Node);
        }

        public void Add(IAstNode node, string message)
        {
            Add(node, message, ValidationErrorLocation.Node);
        }

        public void Add(RToken token, string message, ValidationErrorLocation location)
        {
            Add(new ValidationError(token, message, location));
        }

        public void Add(IAstNode node, string message, ValidationErrorLocation location)
        {
            Add(new ValidationError(node, message, location));
        }
    }
}

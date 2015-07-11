using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Editor.Validation.Definitions;

namespace Microsoft.R.Editor.Validation.Errors
{
    public static class ValidationErrorLocationRange
    {
        public static ITextRange GetRange(IAstNode node, IValidationError error)
        {
            ITextRange range = null;

            switch (error.Location)
            {
                case ValidationErrorLocation.Node:
                    range = new TextRange(node);
                    break;

                case ValidationErrorLocation.BeforeNode:
                    range = new TextRange(node.Start, node.Start);
                    break;

                case ValidationErrorLocation.AfterNode:
                    range = new TextRange(node.End, node.End);
                    break;
            }

            return range;
        }

        private static ITextRange GetTrimmedRange(IAstNode node)
        {
            var text = node.Root.TextProvider.GetText(node);
            var trimmed = text.TrimEnd();

            return new TextRange(node.Start, trimmed.Length);
        }
    }
}

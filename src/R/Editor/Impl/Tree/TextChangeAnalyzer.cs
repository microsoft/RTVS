using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Editor.Tree
{
    internal static class TextChangeAnalyzer
    {
        private static char[] _lineBreaks = new char[] { '\n', '\r' };
        private static char[] _stringSensitiveCharacters = new char[] { '\\', '\'', '\"' };

        public static void DetermineChangeType(TextChangeContext change)
        {
            if (SafeChangeInsideComment(change))
            {
                change.TextChange.TextChangeType |= TextChangeType.Comment;
                return;
            }

            IAstNode node;
            PositionType positionType;

            if (SafeChangeInsideString(change, out node, out positionType))
            {
                change.TextChange.TextChangeType |= TextChangeType.Token;
                return;
            }

            if (SafeWhiteSpaceChange(change, node, positionType))
            {
                change.TextChange.TextChangeType = TextChangeType.Trivial;
                return;
            }

            change.TextChange.TextChangeType = TextChangeType.Structure;
            change.TextChange.FullParseRequired = true;
        }

        private static bool SafeWhiteSpaceChange(TextChangeContext context, IAstNode node, PositionType positionType)
        {
            // In R there is no line continuation so expression may change
            // when user adds or deletes line breaks.

            context.ChangedNode = node;


            if (string.IsNullOrWhiteSpace(context.OldText) && string.IsNullOrWhiteSpace(context.NewText))
            {
                string oldLineText = context.OldTextProvider.GetText(new TextRange(context.OldStart, context.OldLength));
                string newLineText = context.NewTextProvider.GetText(new TextRange(context.Start, context.NewLength));

                if (string.IsNullOrWhiteSpace(oldLineText) && string.IsNullOrWhiteSpace(newLineText) &&
                    oldLineText.IndexOfAny(_lineBreaks) < 0 && newLineText.IndexOfAny(_lineBreaks) < 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool SafeChangeInsideComment(TextChangeContext context)
        {
            var comments = context.EditorTree.AstRoot.Comments;

            int index = comments.GetItemContaining(context.Start);
            if (index < 0)
            {
                return false;
            }

            // Make sure user is not deleting leading # effectively 
            // destroying the comment
            if (comments[index].Start == context.Start && context.OldLength > 0)
            {
                return false;
            }

            // The collection will return a match if the comment starts 
            // at the requested location. However, the change is not 
            // inside the comment if it's at the comment start and 
            // the old length of the change is zero.

            if (comments[index].Start == context.Start && context.OldLength == 0)
            {
                if (context.NewText.IndexOf('#') < 0)
                {
                    context.ChangedComment = comments[index];
                    return true;
                }
            }

            if (context.NewText.IndexOfAny(_lineBreaks) >= 0)
            {
                return false;
            }

            // The change is not safe if old or new text contains line breaks
            // as in R comments runs to the end of the line and deleting
            // line break at the end of the comment may bring code into 
            // the comment range and change the entire file structure.

            if (context.OldText.IndexOfAny(_lineBreaks) >= 0)
            {
                return false;
            }

            context.ChangedComment = comments[index];
            return true;
        }

        private static bool SafeChangeInsideString(TextChangeContext context, out IAstNode node, out PositionType positionType)
        {
            positionType = context.EditorTree.AstRoot.GetPositionNode(context.Start, out node);

            if (positionType != PositionType.Token)
            {
                return false;
            }

            TokenNode tokenNode = node as TokenNode;
            Debug.Assert(tokenNode != null);

            if (tokenNode.Token.TokenType != RTokenType.String)
            {
                return false;
            }

            if (context.OldText.IndexOfAny(_stringSensitiveCharacters) >= 0)
            {
                return false;
            }

            if (context.NewText.IndexOfAny(_stringSensitiveCharacters) >= 0)
            {
                return false;
            }

            context.ChangedNode = node;
            return true;
        }
    }
}

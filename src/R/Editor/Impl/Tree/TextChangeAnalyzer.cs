using System.Diagnostics;
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
                change.TextChange.TextChangeType |= TextChangeType.Token;
                return;
            }

            IAstNode node;
            PositionType positionType;

            if (SafeChangeInsideString(change, out node, out positionType))
            {
                change.TextChange.TextChangeType |= TextChangeType.Token;
                return;
            }

            if (SafeLineBreak(change, node, positionType))
            {
                change.TextChange.TextChangeType = TextChangeType.Trivial;
                return;
            }

            change.TextChange.TextChangeType = TextChangeType.Structure;
            change.TextChange.FullParseRequired = true;
        }

        private static bool SafeLineBreak(TextChangeContext context, IAstNode node, PositionType positionType)
        {
            // In R there is no line continuation and expression hence
            // adding or deleting line breaks may change expression syntax.

            context.ChangedNode = node;

            if (positionType == PositionType.Undefined || positionType == PositionType.Scope)
            {
                return true;
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

            return true;
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
                    context.ChangedNode = comments[index];
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

            context.ChangedNode = comments[index];
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

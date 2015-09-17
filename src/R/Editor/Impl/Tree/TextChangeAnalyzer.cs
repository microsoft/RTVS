using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Statements.Conditionals;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Editor.Tree
{
    internal static class TextChangeAnalyzer
    {
        private static char[] _lineBreaks = new char[] { '\n', '\r' };
        private static char[] _stringSensitiveCharacters = new char[] { '\\', '\'', '\"' };

        public static void DetermineChangeType(TextChangeContext change)
        {
            change.TextChange.TextChangeType |= CheckChangeInsideComment(change);
            if (change.TextChange.TextChangeType == TextChangeType.Comment)
            {
                return;
            }
            else if (change.TextChange.TextChangeType == TextChangeType.Trivial)
            {
                IAstNode node;
                PositionType positionType;

                change.TextChange.TextChangeType |= CheckChangeInsideString(change, out node, out positionType);
                if (change.TextChange.TextChangeType == TextChangeType.Token)
                {
                    return;
                }
                else if (change.TextChange.TextChangeType == TextChangeType.Trivial)
                {
                    change.TextChange.TextChangeType |= CheckWhiteSpaceChange(change, node, positionType);
                    if (change.TextChange.TextChangeType == TextChangeType.Trivial)
                    {
                        return;
                    }
                }
            }

            change.TextChange.TextChangeType = TextChangeType.Structure;
            change.TextChange.FullParseRequired = true;
        }

        private static TextChangeType CheckWhiteSpaceChange(TextChangeContext context, IAstNode node, PositionType positionType)
        {
            context.ChangedNode = node;

            if (string.IsNullOrWhiteSpace(context.OldText) && string.IsNullOrWhiteSpace(context.NewText))
            {
                // In R there is no line continuation so expression may change when user adds or deletes line breaks.
                bool lineBreakSensitive = (node is If) && ((If)node).LineBreakSensitive;
                if (lineBreakSensitive)
                {
                    string oldLineText = context.OldTextProvider.GetText(new TextRange(context.OldStart, context.OldLength));
                    string newLineText = context.NewTextProvider.GetText(new TextRange(context.Start, context.NewLength));

                    if (oldLineText.IndexOfAny(_lineBreaks) >= 0 || newLineText.IndexOfAny(_lineBreaks) >= 0)
                    {
                        return TextChangeType.Structure;
                    }
                }

                return TextChangeType.Trivial;
            }

            return TextChangeType.Structure;
        }

        private static TextChangeType CheckChangeInsideComment(TextChangeContext context)
        {
            var comments = context.EditorTree.AstRoot.Comments;

            IReadOnlyList<int> affectedComments = comments.GetItemsContainingInclusiveEnd(context.Start);
            if (affectedComments.Count == 0)
            {
                return TextChangeType.Trivial;
            }

            if (affectedComments.Count > 1)
            {
                return TextChangeType.Structure;
            }

            // Make sure user is not deleting leading # effectively 
            // destroying the comment
            RToken comment = comments[affectedComments[0]];
            if (comment.Start == context.Start && context.OldLength > 0)
            {
                return TextChangeType.Structure;
            }

            // The collection will return a match if the comment starts 
            // at the requested location. However, the change is not 
            // inside the comment if it's at the comment start and 
            // the old length of the change is zero.

            if (comment.Start == context.Start && context.OldLength == 0)
            {
                if (context.NewText.IndexOf('#') < 0)
                {
                    context.ChangedComment = comment;
                    return TextChangeType.Comment;
                }
            }

            if (context.NewText.IndexOfAny(_lineBreaks) >= 0)
            {
                return TextChangeType.Structure;
            }

            // The change is not safe if old or new text contains line breaks
            // as in R comments runs to the end of the line and deleting
            // line break at the end of the comment may bring code into 
            // the comment range and change the entire file structure.

            if (context.OldText.IndexOfAny(_lineBreaks) >= 0)
            {
                return TextChangeType.Structure;
            }

            context.ChangedComment = comment;
            return TextChangeType.Comment;
        }

        private static TextChangeType CheckChangeInsideString(TextChangeContext context, out IAstNode node, out PositionType positionType)
        {
            positionType = context.EditorTree.AstRoot.GetPositionNode(context.Start, out node);

            if (positionType == PositionType.Token)
            {
                TokenNode tokenNode = node as TokenNode;
                Debug.Assert(tokenNode != null);

                if (tokenNode.Token.TokenType == RTokenType.String)
                {

                    if (context.OldText.IndexOfAny(_stringSensitiveCharacters) >= 0)
                    {
                        return TextChangeType.Structure;
                    }

                    if (context.NewText.IndexOfAny(_stringSensitiveCharacters) >= 0)
                    {
                        return TextChangeType.Structure;
                    }

                    context.ChangedNode = node;
                    return TextChangeType.Token;
                }
            }

            return TextChangeType.Trivial;
        }
    }
}

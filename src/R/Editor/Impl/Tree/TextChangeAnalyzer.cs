// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Statements.Conditionals;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Editor.Tree {
    internal static class TextChangeAnalyzer {
        private static char[] _stringSensitiveCharacters = new char[] { '\\', '\'', '\"' };

        public static void DetermineChangeType(TextChangeContext change) {
            change.PendingChanges.TextChangeType |= CheckChangeInsideComment(change);
            if (change.PendingChanges.TextChangeType == TextChangeType.Comment) {
                return;
            } else if (change.PendingChanges.TextChangeType == TextChangeType.Trivial) {
                IAstNode node;
                PositionType positionType;

                change.PendingChanges.TextChangeType |= CheckChangeInsideString(change, out node, out positionType);
                if (change.PendingChanges.TextChangeType == TextChangeType.Token) {
                    return;
                } else if (node != null && change.PendingChanges.TextChangeType == TextChangeType.Trivial) {
                    change.PendingChanges.TextChangeType |= CheckWhiteSpaceChange(change, node, positionType);
                    if (change.PendingChanges.TextChangeType == TextChangeType.Trivial) {
                        return;
                    }
                }
            }
            change.PendingChanges.TextChangeType = TextChangeType.Structure;
            change.PendingChanges.FullParseRequired = true;
        }

        private static TextChangeType CheckWhiteSpaceChange(TextChangeContext context, IAstNode node, PositionType positionType) {
            context.ChangedNode = node;

            if (string.IsNullOrWhiteSpace(context.OldText) && string.IsNullOrWhiteSpace(context.NewText)) {
                // In R there is no line continuation so expression may change when user adds or deletes line breaks.
                var lineBreakSensitive = node is If;
                if (lineBreakSensitive) {
                    var oldLineText = context.OldTextProvider.GetText(new TextRange(context.OldStart, context.OldLength));
                    var newLineText = context.NewTextProvider.GetText(new TextRange(context.NewStart, context.NewLength));

                    if (oldLineText.IndexOfAny(CharExtensions.LineBreakChars) >= 0 || newLineText.IndexOfAny(CharExtensions.LineBreakChars) >= 0) {
                        return TextChangeType.Structure;
                    }
                }
                // Change inside token node is destructive: consider adding space inside an indentifier
                if (!IsChangeDestructiveForChildNodes(node, context.OldRange)) {
                    return TextChangeType.Trivial;
                }
            }
            return TextChangeType.Structure;
        }

        private static bool IsChangeDestructiveForChildNodes(IAstNode node, ITextRange changedRange) {
            if(changedRange.End <= node.Start || changedRange.Start >= node.End) {
                return false;
            }
            else if(node.Children.Count == 0) {
                return true;
            }

            var result = false;
            foreach (var child in node.Children) {
                result |= IsChangeDestructiveForChildNodes(child, changedRange);
                if(result) {
                    break;
                }
            }
            return result;
        }

        private static TextChangeType CheckChangeInsideComment(TextChangeContext context) {
            var comments = context.EditorTree.AstRoot.Comments;

            var affectedComments = comments.GetItemsContainingInclusiveEnd(context.NewStart);
            if (affectedComments.Count == 0) {
                return TextChangeType.Trivial;
            }
            if (affectedComments.Count > 1) {
                return TextChangeType.Structure;
            }

            // Make sure user is not deleting leading # effectively 
            // destroying the comment
            var comment = comments[affectedComments[0]];
            if (comment.Start == context.NewStart && context.OldLength > 0) {
                return TextChangeType.Structure;
            }

            // The collection will return a match if the comment starts 
            // at the requested location. However, the change is not 
            // inside the comment if it's at the comment start and 
            // the old length of the change is zero.

            if (comment.Start == context.NewStart && context.OldLength == 0) {
                if (context.NewText.IndexOf('#') < 0) {
                    context.ChangedComment = comment;
                    return TextChangeType.Comment;
                }
            }

            if (context.NewText.IndexOfAny(CharExtensions.LineBreakChars) >= 0) {
                return TextChangeType.Structure;
            }
            // The change is not safe if old or new text contains line breaks
            // as in R comments runs to the end of the line and deleting
            // line break at the end of the comment may bring code into 
            // the comment range and change the entire file structure.
            if (context.OldText.IndexOfAny(CharExtensions.LineBreakChars) >= 0) {
                return TextChangeType.Structure;
            }

            context.ChangedComment = comment;
            return TextChangeType.Comment;
        }

        private static TextChangeType CheckChangeInsideString(TextChangeContext context, out IAstNode node, out PositionType positionType) {
            positionType = context.EditorTree.AstRoot.GetPositionNode(context.NewStart, out node);

            if (positionType == PositionType.Token) {
                var tokenNode = node as TokenNode;
                Debug.Assert(tokenNode != null);

                if (tokenNode.Token.TokenType == RTokenType.String) {
                    if (context.OldText.IndexOfAny(_stringSensitiveCharacters) >= 0) {
                        return TextChangeType.Structure;
                    }
                    if (context.NewText.IndexOfAny(_stringSensitiveCharacters) >= 0) {
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

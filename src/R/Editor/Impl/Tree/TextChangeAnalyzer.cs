// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Statements.Conditionals;
using Microsoft.R.Core.Tokens;

namespace Microsoft.R.Editor.Tree {
    internal static class TextChangeAnalyzer {
        private static readonly char[] _stringSensitiveCharacters = { '\\', '\'', '\"' };

        public static void DetermineChangeType(TextChangeContext change) {
            var changeType = CheckChangeInsideComment(change);
            change.PendingChanges.TextChangeType = MathExtensions.Max(changeType, change.PendingChanges.TextChangeType);

            switch (change.PendingChanges.TextChangeType) {
                case TextChangeType.Comment:
                    return;

                case TextChangeType.Trivial:
                    changeType = CheckChangeInsideString(change, out var node, out var positionType);
                    change.PendingChanges.TextChangeType = MathExtensions.Max(changeType, change.PendingChanges.TextChangeType);
                    if (change.PendingChanges.TextChangeType == TextChangeType.Token) {
                        return;
                    }

                    if (node != null && change.PendingChanges.TextChangeType == TextChangeType.Trivial) {
                        changeType = CheckWhiteSpaceChange(change, node, positionType);
                        change.PendingChanges.TextChangeType = MathExtensions.Max(changeType, change.PendingChanges.TextChangeType);
                        if (change.PendingChanges.TextChangeType == TextChangeType.Trivial) {
                            return;
                        }
                    }
                    break;
            }

            change.PendingChanges.TextChangeType = TextChangeType.Structure;
            change.PendingChanges.FullParseRequired = true;
        }

        private static TextChangeType CheckWhiteSpaceChange(TextChangeContext context, IAstNode node, PositionType positionType) {
            context.ChangedNode = node;
            var change = context.PendingChanges;

            if (string.IsNullOrWhiteSpace(change.OldText) && string.IsNullOrWhiteSpace(change.NewText)) {
                // In R there is no line continuation so expression may change when user adds or deletes line breaks.
                var lineBreakSensitive = IsLineBreakSensitive(context, node);
                if (lineBreakSensitive) {
                    var oldLineText = change.OldTextProvider.GetText(new TextRange(change.Start, change.OldLength));
                    var newLineText = change.NewTextProvider.GetText(new TextRange(change.Start, change.NewLength));

                    if (oldLineText.IndexOfAny(CharExtensions.LineBreakChars) >= 0 || newLineText.IndexOfAny(CharExtensions.LineBreakChars) >= 0) {
                        return TextChangeType.Structure;
                    }
                }
                // Change inside token node is destructive: consider adding space inside an indentifier
                if (!IsChangeDestructiveForChildNodes(node, change.OldRange)) {
                    return TextChangeType.Trivial;
                }
            }
            return TextChangeType.Structure;
        }

        private static bool IsLineBreakSensitive(TextChangeContext context, IAstNode node) {
            if (node is If) {
                return true;
            }

            var position = context.PendingChanges.Start;
            var candidate = node.Root.GetNodeOfTypeFromPosition<If>(position);
            if (candidate != null) {
                return true;
            }

            // Check if line break is added to removed on a line with closing } of 'if'
            // so we can full-parse if 'else' position changes relatively to the if
            var snapshot = context.EditorTree.EditorBuffer.CurrentSnapshot;
            var text = snapshot.GetLineFromPosition(context.PendingChanges.Start).GetText();

            // We need to find if any position in the line belong to `if` scope
            // if there is 'else' the same line
            return FindKeyword("if", text) || FindKeyword("else", text);
        }

        private static bool FindKeyword(string keyword, string text)
            => new RTokenizer()
                .Tokenize(text)
                .FirstOrDefault(t => 
                        t.TokenType == RTokenType.Keyword && 
                        string.Compare(text, t.Start, keyword, 0, keyword.Length, StringComparison.Ordinal) == 0) != null;

        private static bool IsChangeDestructiveForChildNodes(IAstNode node, ITextRange changedRange) {
            if (changedRange.End <= node.Start || changedRange.Start >= node.End) {
                return false;
            }
            if (node.Children.Count == 0) {
                return true;
            }

            var result = false;
            foreach (var child in node.Children) {
                result |= IsChangeDestructiveForChildNodes(child, changedRange);
                if (result) {
                    break;
                }
            }
            return result;
        }

        private static TextChangeType CheckChangeInsideComment(TextChangeContext context) {
            var comments = context.EditorTree.AstRoot.Comments;
            var change = context.PendingChanges;

            var affectedComments = comments.GetItemsContainingInclusiveEnd(change.Start);
            if (affectedComments.Count == 0) {
                return TextChangeType.Trivial;
            }
            if (affectedComments.Count > 1) {
                return TextChangeType.Structure;
            }

            // Make sure user is not deleting leading # effectively 
            // destroying the comment
            var comment = comments[affectedComments[0]];
            if (comment.Start == change.Start && change.OldLength > 0) {
                return TextChangeType.Structure;
            }

            // The collection will return a match if the comment starts 
            // at the requested location. However, the change is not 
            // inside the comment if it's at the comment start and 
            // the old length of the change is zero.

            if (comment.Start == change.Start && change.OldLength == 0) {
                if (change.NewText.IndexOf('#') < 0) {
                    context.ChangedComment = comment;
                    return TextChangeType.Comment;
                }
            }

            if (change.NewText.IndexOfAny(CharExtensions.LineBreakChars) >= 0) {
                return TextChangeType.Structure;
            }
            // The change is not safe if old or new text contains line breaks
            // as in R comments runs to the end of the line and deleting
            // line break at the end of the comment may bring code into 
            // the comment range and change the entire file structure.
            if (change.OldText.IndexOfAny(CharExtensions.LineBreakChars) >= 0) {
                return TextChangeType.Structure;
            }

            context.ChangedComment = comment;
            return TextChangeType.Comment;
        }

        private static TextChangeType CheckChangeInsideString(TextChangeContext context, out IAstNode node, out PositionType positionType) {
            var change = context.PendingChanges;
            positionType = context.EditorTree.AstRoot.GetPositionNode(change.Start, out node);

            if (positionType == PositionType.Token && change.Start > node.Start) {
                var tokenNode = node as TokenNode;
                Debug.Assert(tokenNode != null);

                if (tokenNode.Token.TokenType == RTokenType.String) {
                    if (change.OldText.IndexOfAny(_stringSensitiveCharacters) >= 0) {
                        return TextChangeType.Structure;
                    }
                    if (change.NewText.IndexOfAny(_stringSensitiveCharacters) >= 0) {
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

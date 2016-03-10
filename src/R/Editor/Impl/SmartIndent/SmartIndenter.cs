// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Editor.Services;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Functions.Definitions;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.AST.Statements.Definitions;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Document.Definitions;
using Microsoft.R.Editor.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.SmartIndent {
    /// <summary>
    /// Provides block and smart indentation in R code
    /// </summary>
    internal sealed class SmartIndenter : ISmartIndent {
        private ITextView _textView;

        public static SmartIndenter Attach(ITextView textView) {
            SmartIndenter indenter = ServiceManager.GetService<SmartIndenter>(textView);

            if (indenter == null) {
                indenter = new SmartIndenter(textView);
            }

            return indenter;
        }

        private SmartIndenter(ITextView textView) {
            _textView = textView;
        }

        #region ISmartIndent;
        public int? GetDesiredIndentation(ITextSnapshotLine line) {
            int? res = GetDesiredIndentation(line, REditorSettings.IndentStyle);
            if (res != null && line.Snapshot.TextBuffer != _textView.TextBuffer) {
                var target = _textView.BufferGraph.MapUpToBuffer(
                    line.Start,
                    PointTrackingMode.Positive,
                    PositionAffinity.Successor,
                    _textView.TextBuffer
                );

                if (target != null) {
                    // The indentation level is relative to the line in the text view when
                    // we were created, not to the line we were provided with on this call.
                    var diff = target.Value.Position - target.Value.GetContainingLine().Start.Position;
                    return diff + res;
                }
            }
            return res;
        }

        public int? GetDesiredIndentation(ITextSnapshotLine line, IndentStyle indentStyle) {
            if (line != null) {
                if (indentStyle == IndentStyle.Block) {
                    return GetBlockIndent(line);
                } else if (indentStyle == IndentStyle.Smart) {
                    return GetSmartIndent(line);
                }
            }

            return null;
        }

        public void Dispose() {
        }
        #endregion

        public static int GetBlockIndent(ITextSnapshotLine line) {
            int lineNumber = line.LineNumber;

            //Scan the previous lines for the first line that isn't an empty line.
            while (--lineNumber >= 0) {
                ITextSnapshotLine previousLine = line.Snapshot.GetLineFromLineNumber(lineNumber);
                if (previousLine.Length > 0) {
                    return OuterIndentSizeFromLine(previousLine, REditorSettings.FormatOptions);
                }
            }

            return 0;
        }

        public static int GetSmartIndent(ITextSnapshotLine line, AstRoot ast = null) {
            ITextBuffer textBuffer = line.Snapshot.TextBuffer;
            ITextSnapshotLine prevLine = null;

            if (ast == null) {
                IREditorDocument document = REditorDocument.TryFromTextBuffer(textBuffer);
                if (document == null) {
                    return 0;
                }
                ast = document.EditorTree.AstRoot;
            }

            if (line.LineNumber > 0) {
                prevLine = line.Snapshot.GetLineFromLineNumber(line.LineNumber - 1);
                string prevLineText = prevLine.GetText();
                int nonWsPosition = prevLine.Start + (prevLineText.Length - prevLineText.TrimStart().Length) + 1;

                IAstNodeWithScope scopeStatement = ast.GetNodeOfTypeFromPosition<IAstNodeWithScope>(nonWsPosition);
                if (scopeStatement == null) {
                    // Line start position works for typical scope-defining statements like if() or while()
                    // but it won't find function definition in x <- function(a) { ...
                    // Try end of the line instead
                    nonWsPosition = Math.Max(0, prevLineText.TrimEnd().Length - 1);
                    scopeStatement = ast.GetNodeOfTypeFromPosition<IAstNodeWithScope>(nonWsPosition);
                }

                if (scopeStatement != null) {
                    if (scopeStatement.Scope == null) {
                        // No scope of any kind, use block indent
                        return GetBlockIndent(line) + REditorSettings.IndentSize;
                    }

                    if (scopeStatement.Scope is SimpleScope) {
                        // There is statement with a simple scope above. We need to check 
                        // if the line that is being formatted is actually part of this scope.
                        if (line.Start < scopeStatement.Scope.End) {
                            // Indent line one level deeper that the statement
                            return GetBlockIndent(line) + REditorSettings.IndentSize;
                        }

                        // Line is not part of the scope, hence regular indent
                        return OuterIndentSizeFromNode(textBuffer, scopeStatement, REditorSettings.FormatOptions);
                    }

                    // Check if line is the last line in scope and if so, 
                    // it should be indented at the outer indent
                    if (scopeStatement.Scope.CloseCurlyBrace != null) {
                        int endOfScopeLine = textBuffer.CurrentSnapshot.GetLineNumberFromPosition(scopeStatement.Scope.CloseCurlyBrace.Start);
                        if (endOfScopeLine == line.LineNumber) {
                            return OuterIndentSizeFromNode(textBuffer, scopeStatement, REditorSettings.FormatOptions);
                        }
                    }

                    return InnerIndentSizeFromNode(textBuffer, scopeStatement, REditorSettings.FormatOptions);
                }
            }

            IAstNodeWithScope node = ast.GetNodeOfTypeFromPosition<IAstNodeWithScope>(line.Start);
            if (node != null && node.Scope != null && node.Scope.OpenCurlyBrace != null) {
                return InnerIndentSizeFromNode(textBuffer, node, REditorSettings.FormatOptions);
            }

            // See if we are in function arguments and indent at the function level
            var fc = ast.GetNodeOfTypeFromPosition<IFunction>(line.Start);
            if (fc != null && fc.Arguments != null && fc.OpenBrace != null && line.Start >= fc.OpenBrace.End) {
                return GetFirstArgumentIndent(textBuffer.CurrentSnapshot, fc);
            }

            // We can be at the end of the incomplete function call line just pressed Enter after func(a,
            // Let's see if this is the case
            if (prevLine != null && prevLine.Length > 0) {
                fc = ast.GetNodeOfTypeFromPosition<IFunction>(prevLine.End - 1);
                if (fc != null && fc.Arguments != null && fc.OpenBrace != null && fc.CloseBrace == null && line.Start >= fc.OpenBrace.End) {
                    return GetFirstArgumentIndent(textBuffer.CurrentSnapshot, fc);
                }
            }

            return 0;
        }

        private static int GetFirstArgumentIndent(ITextSnapshot snapshot, IFunction fc) {
            var line = snapshot.GetLineFromPosition(fc.OpenBrace.End);
            return fc.OpenBrace.End - line.Start;
        }

        public static int InnerIndentSizeFromNode(ITextBuffer textBuffer, IAstNode node, RFormatOptions options) {
            if (node != null) {
                ITextSnapshotLine startLine = textBuffer.CurrentSnapshot.GetLineFromPosition(node.Start);
                return InnerIndentSizeFromLine(startLine, options);
            }

            return 0;
        }

        public static int OuterIndentSizeFromNode(ITextBuffer textBuffer, IAstNode node, RFormatOptions options) {
            if (node != null) {
                ITextSnapshotLine startLine = textBuffer.CurrentSnapshot.GetLineFromPosition(node.Start);
                return OuterIndentSizeFromLine(startLine, options);
            }

            return 0;
        }

        public static int InnerIndentSizeFromLine(ITextSnapshotLine line, RFormatOptions options) {
            string lineText = line.GetText();
            string leadingWhitespace = lineText.Substring(0, lineText.Length - lineText.TrimStart().Length);
            IndentBuilder indentbuilder = new IndentBuilder(options.IndentType, options.IndentSize, options.TabSize);

            return IndentBuilder.TextIndentInSpaces(leadingWhitespace + indentbuilder.SingleIndentString, options.TabSize);
        }

        public static int OuterIndentSizeFromLine(ITextSnapshotLine line, RFormatOptions options) {
            string lineText = line.GetText();
            string leadingWhitespace = lineText.Substring(0, lineText.Length - lineText.TrimStart().Length);

            return IndentBuilder.TextIndentInSpaces(leadingWhitespace, options.TabSize);
        }
    }
}

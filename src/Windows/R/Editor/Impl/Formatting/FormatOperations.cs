// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Core.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.AST.Statements;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.SmartIndent;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting {
    internal static class FormatOperations {
        /// <summary>
        /// Formats statement that the caret is at
        /// </summary>
        public static void FormatCurrentStatement(ITextView textView, ITextBuffer textBuffer, ICoreShell shell, bool limitAtCaret = false, int caretOffset = 0) {
            SnapshotPoint? caretPoint = REditorDocument.MapCaretPositionFromView(textView);
            if (!caretPoint.HasValue) {
                return;
            }
            IREditorDocument document = REditorDocument.TryFromTextBuffer(textBuffer);
            if (document != null) {
                ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
                AstRoot ast = document.EditorTree.AstRoot;
                IAstNode node = ast.GetNodeOfTypeFromPosition<IStatement>(Math.Max(0, caretPoint.Value + caretOffset)) as IAstNode;
                FormatNode(textView, textBuffer, shell, node, limit: caretPoint.Value);
            }
        }

        /// <summary>
        /// Formats specific AST node 
        /// </summary>
        public static void FormatNode(ITextView textView, ITextBuffer textBuffer, ICoreShell shell, IAstNode node, int limit = -1) {
            if (node != null) {
                if (limit >= 0 && limit < node.Start) {
                    throw new ArgumentException(nameof(limit));
                }
                var range = limit < 0 ? node as ITextRange : TextRange.FromBounds(node.Start, limit);
                UndoableFormatRange(textView, textBuffer, range, shell);
            }
        }

        public static void FormatCurrentScope(ITextView textView, ITextBuffer textBuffer, ICoreShell shell, bool indentCaret) {
            // Figure out caret position in the document text buffer
            var caretPoint = REditorDocument.MapCaretPositionFromView(textView);
            if (!caretPoint.HasValue) {
                return;
            }
            var document = REditorDocument.TryFromTextBuffer(textBuffer);
            if (document != null) {
                // Make sure AST is up to date
                document.EditorTree.EnsureTreeReady();
                var ast = document.EditorTree.AstRoot;
                var snapshot = textBuffer.CurrentSnapshot;

                // Find scope to format
                var scope = ast.GetNodeOfTypeFromPosition<IScope>(caretPoint.Value);

                var es = shell.GetService<IApplicationEditorSupport>();
                using (var undoAction = es.CreateCompoundAction(textView, textView.TextBuffer)) {
                    var settings = shell.GetService<IREditorSettings>();
                    undoAction.Open(Resources.AutoFormat);
                    // Now format the scope
                    bool changed = RangeFormatter.FormatRange(textView, textBuffer, scope, settings.FormatOptions, shell);
                    if (indentCaret) {
                        // Formatting may change AST and the caret position so we need to reacquire both
                        caretPoint = REditorDocument.MapCaretPositionFromView(textView);
                        if (caretPoint.HasValue) {
                            document.EditorTree.EnsureTreeReady();
                            ast = document.EditorTree.AstRoot;
                            scope = ast.GetNodeOfTypeFromPosition<IScope>(caretPoint.Value);
                            IndentCaretInNewScope(textView, scope, caretPoint.Value, settings.FormatOptions);
                        }
                    }
                    if (changed) {
                        undoAction.Commit();
                    }
                }
            }
        }

        /// <summary>
        /// Formats line the caret is currently at
        /// </summary>
        public static void FormatViewLine(ITextView textView, ITextBuffer textBuffer, int offset, ICoreShell shell) {
            var caretPoint = REditorDocument.MapCaretPositionFromView(textView);
            if (!caretPoint.HasValue) {
                return;
            }

            var snapshot = textBuffer.CurrentSnapshot;
            var lineNumber = snapshot.GetLineNumberFromPosition(caretPoint.Value.Position);
            var line = snapshot.GetLineFromLineNumber(lineNumber + offset);
            var formatRange = new TextRange(line.Start, line.Length);

            UndoableFormatRange(textView, textBuffer, formatRange, shell);
        }

        public static void UndoableFormatRange(ITextView textView, ITextBuffer textBuffer, ITextRange formatRange, ICoreShell shell) {
            var es = shell.GetService<IApplicationEditorSupport>();
            using (var undoAction = es.CreateCompoundAction(textView, textView.TextBuffer)) {
                undoAction.Open(Resources.AutoFormat);
                var result = RangeFormatter.FormatRange(textView, textBuffer, formatRange, shell.GetService<IREditorSettings>().FormatOptions, shell);
                if (result) {
                    undoAction.Commit();
                }
            }
        }

        public static IAstNode GetIndentDefiningNode(AstRoot ast, int position) {
            var scope = ast.GetNodeOfTypeFromPosition<IScope>(position);
            // Scope indentation is defined by its parent statement.
            var parentStatement = scope.Parent as IAstNodeWithScope;
            if (parentStatement != null && parentStatement.Scope == scope) {
                return parentStatement;
            }
            return scope;
        }

        private static void IndentCaretInNewScope(ITextView textView, IScope scope, SnapshotPoint caretBufferPoint, RFormatOptions options) {
            if (scope == null || scope.OpenCurlyBrace == null) {
                return;
            }
            var rSnapshot = caretBufferPoint.Snapshot;
            var rTextBuffer = rSnapshot.TextBuffer;
            var rCaretPosition = caretBufferPoint.Position;

            var caretLine = rSnapshot.GetLineFromPosition(rCaretPosition);
            var innerIndentSize = SmartIndenter.InnerIndentSizeFromNode(rTextBuffer, scope, options);

            var openBraceLineNumber = rSnapshot.GetLineNumberFromPosition(scope.OpenCurlyBrace.Start);
            var braceLine = rSnapshot.GetLineFromLineNumber(openBraceLineNumber);
            var indentLine = rSnapshot.GetLineFromLineNumber(openBraceLineNumber + 1);

            var lineBreakText = braceLine.GetLineBreakText();
            rTextBuffer.Insert(indentLine.Start, lineBreakText);

            // Fetch the line again since snapshot has changed when line break was inserted
            indentLine = rTextBuffer.CurrentSnapshot.GetLineFromLineNumber(openBraceLineNumber + 1);
            
            // Map new caret position back to the view
            var positionInView = textView.MapUpToView(indentLine.Start);
            if (positionInView.HasValue) {
                var viewIndentLine = textView.TextBuffer.CurrentSnapshot.GetLineFromPosition(positionInView.Value);
                textView.Caret.MoveTo(new VirtualSnapshotPoint(viewIndentLine.Start, innerIndentSize));
            }
        }
    }
}

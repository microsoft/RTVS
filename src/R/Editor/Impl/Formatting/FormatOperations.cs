// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.AST.Statements;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.SmartIndent;

namespace Microsoft.R.Editor.Formatting {
    public static class FormatOperations {
        /// <summary>
        /// Formats statement that the caret is at
        /// </summary>
        public static void FormatCurrentStatement(IEditorView editorView, IEditorBuffer textBuffer, IServiceContainer services, bool limitAtCaret = false, int caretOffset = 0) {
            var caretPoint = editorView.GetCaretPosition(textBuffer);
            if (caretPoint == null) {
                return;
            }
            var document = textBuffer.GetEditorDocument<IREditorDocument>();
            if (document != null) {
                var ast = document.EditorTree.AstRoot;
                var node = ast.GetNodeOfTypeFromPosition<IStatement>(Math.Max(0, caretPoint.Position + caretOffset)) as IAstNode;
                FormatNode(editorView, textBuffer, services, node, limit: caretPoint.Position);
            }
        }

        /// <summary>
        /// Formats specific AST node 
        /// </summary>
        public static void FormatNode(IEditorView editorView, IEditorBuffer textBuffer, IServiceContainer services, IAstNode node, int limit = -1) {
            if (node != null) {
                if (limit >= 0 && limit < node.Start) {
                    throw new ArgumentException(nameof(limit));
                }
                var range = limit < 0 ? node as ITextRange : TextRange.FromBounds(node.Start, limit);
                UndoableFormatRange(editorView, textBuffer, range, services);
            }
        }

        public static void FormatCurrentScope(IEditorView editorView, IEditorBuffer textBuffer, IServiceContainer services, bool indentCaret) {
            // Figure out caret position in the document text buffer
            var document = textBuffer.GetEditorDocument<IREditorDocument>();
            if (document == null) {
                return;
            }
            var caretPoint = editorView.GetCaretPosition(textBuffer);
            if (caretPoint == null) {
                return;
            }

            // Make sure AST is up to date
            document.EditorTree.EnsureTreeReady();
            var ast = document.EditorTree.AstRoot;
            // Find scope to format
            var scope = ast.GetNodeOfTypeFromPosition<IScope>(caretPoint.Position);

            var es = services.GetService<IEditorSupport>();
            using (var undoAction = es.CreateUndoAction(editorView)) {
                var settings = services.GetService<IREditorSettings>();
                undoAction.Open(Resources.AutoFormat);
                // Now format the scope
                var formatter = new RangeFormatter(services);
                var changed = formatter.FormatRange(editorView, textBuffer, scope);
                if (indentCaret) {
                    // Formatting may change AST and the caret position so we need to reacquire both
                    caretPoint = editorView.GetCaretPosition(textBuffer);
                    if (caretPoint != null) {
                        document.EditorTree.EnsureTreeReady();
                        ast = document.EditorTree.AstRoot;
                        scope = ast.GetNodeOfTypeFromPosition<IScope>(caretPoint.Position);
                        IndentCaretInNewScope(editorView, scope, caretPoint, settings.FormatOptions);
                    }
                }
                if (changed) {
                    undoAction.Commit();
                }
            }
        }

        /// <summary>
        /// Formats line the caret is currently at
        /// </summary>
        public static void FormatViewLine(IEditorView editorView, IEditorBuffer textBuffer, int offset, IServiceContainer services) {
            var caretPoint = editorView.GetCaretPosition(textBuffer);
            if (caretPoint == null) {
                return;
            }

            var snapshot = textBuffer.CurrentSnapshot;
            var lineNumber = snapshot.GetLineNumberFromPosition(caretPoint.Position);
            var line = snapshot.GetLineFromLineNumber(lineNumber + offset);
            var formatRange = new TextRange(line.Start, line.Length);

            UndoableFormatRange(editorView, textBuffer, formatRange, services);
        }

        public static void UndoableFormatRange(IEditorView editorView, IEditorBuffer textBuffer, ITextRange formatRange, IServiceContainer services) {
            var es = services.GetService<IEditorSupport>();
            using (var undoAction = es.CreateUndoAction(editorView)) {
                undoAction.Open(Resources.AutoFormat);
                var formatter = new RangeFormatter(services);
                var result = formatter.FormatRange(editorView, textBuffer, formatRange);
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

        private static void IndentCaretInNewScope(IEditorView editorView, IScope scope, ISnapshotPoint caretBufferPoint, RFormatOptions options) {
            if (scope?.OpenCurlyBrace == null) {
                return;
            }
            var rSnapshot = caretBufferPoint.Snapshot;
            var rTextBuffer = rSnapshot.EditorBuffer;
            var innerIndentSize = SmartIndenter.InnerIndentSizeFromNode(rTextBuffer, scope, options);

            var openBraceLineNumber = rSnapshot.GetLineNumberFromPosition(scope.OpenCurlyBrace.Start);
            var braceLine = rSnapshot.GetLineFromLineNumber(openBraceLineNumber);
            var indentLine = rSnapshot.GetLineFromLineNumber(openBraceLineNumber + 1);

            var lineBreakText = braceLine.LineBreak;
            rTextBuffer.Insert(indentLine.Start, lineBreakText);

            // Fetch the line again since snapshot has changed when line break was inserted
            indentLine = rTextBuffer.CurrentSnapshot.GetLineFromLineNumber(openBraceLineNumber + 1);

            // Map new caret position back to the view
            var positionInView = editorView.MapToView(rTextBuffer.CurrentSnapshot, indentLine.Start);
            if (positionInView != null) {
                var viewIndentLine = editorView.EditorBuffer.CurrentSnapshot.GetLineFromPosition(positionInView.Position);
                editorView.Caret.MoveTo(viewIndentLine.Start, innerIndentSize);
            }
        }
    }
}

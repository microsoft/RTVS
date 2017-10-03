// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Formatting;
using Microsoft.Languages.Editor.Text;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.AST.Statements;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.SmartIndent;

namespace Microsoft.R.Editor.Formatting {
    public sealed class FormatOperations {
        private readonly IServiceContainer _services;
        private readonly IEditorView _editorView;
        private readonly IEditorBuffer _editorBuffer;
        private readonly IIncrementalWhitespaceChangeHandler _changeHandler;

        public FormatOperations(IServiceContainer services, IEditorView editorView, IEditorBuffer editorBuffer, IIncrementalWhitespaceChangeHandler changeHandler = null) {
            _services = services;
            _editorView = editorView;
            _editorBuffer = editorBuffer;
            _changeHandler = changeHandler ?? _services.GetService<IIncrementalWhitespaceChangeHandler>();
        }

        /// <summary>
        /// Formats statement that the caret is at
        /// </summary>
        public void FormatCurrentStatement(bool limitAtCaret = false, int caretOffset = 0) {
            var caretPoint = _editorView.GetCaretPosition(_editorBuffer);
            if (caretPoint == null) {
                return;
            }
            var document = _editorBuffer.GetEditorDocument<IREditorDocument>();
            if (document != null) {
                var ast = document.EditorTree.AstRoot;
                var node = ast.GetNodeOfTypeFromPosition<IStatement>(Math.Max(0, caretPoint.Position + caretOffset)) as IAstNode;
                FormatNode(node, limit: caretPoint.Position);
            }
        }

        /// <summary>
        /// Formats specific AST node 
        /// </summary>
        public void FormatNode(IAstNode node, int limit = -1) {
            if (node != null) {
                if (limit >= 0 && limit < node.Start) {
                    throw new ArgumentOutOfRangeException(nameof(limit));
                }
                var range = limit < 0 ? node as ITextRange : TextRange.FromBounds(node.Start, limit);
                UndoableFormatRange(range);
            }
        }

        public void FormatCurrentScope(bool indentCaret) {
            // Figure out caret position in the document text buffer
            var document = _editorBuffer.GetEditorDocument<IREditorDocument>();
            if (document == null) {
                return;
            }
            var caretPoint = _editorView.GetCaretPosition(_editorBuffer);
            if (caretPoint == null) {
                return;
            }

            // Make sure AST is up to date
            document.EditorTree.EnsureTreeReady();
            var ast = document.EditorTree.AstRoot;
            // Find scope to format
            var scope = ast.GetNodeOfTypeFromPosition<IScope>(caretPoint.Position);

            var es = _services.GetService<IEditorSupport>();
            using (var undoAction = es.CreateUndoAction(_editorView)) {
                var settings = _services.GetService<IREditorSettings>();
                undoAction.Open(Resources.AutoFormat);
                // Now format the scope
                var formatter = new RangeFormatter(_services, _editorView, _editorBuffer, _changeHandler);
                var changed = formatter.FormatRange(scope);
                if (indentCaret) {
                    // Formatting may change AST and the caret position so we need to reacquire both
                    caretPoint = _editorView.GetCaretPosition(_editorBuffer);
                    if (caretPoint != null) {
                        document.EditorTree.EnsureTreeReady();
                        ast = document.EditorTree.AstRoot;
                        scope = ast.GetNodeOfTypeFromPosition<IScope>(caretPoint.Position);
                        IndentCaretInNewScope(scope, caretPoint, settings.FormatOptions);
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
        public void FormatViewLine(int offset) {
            var caretPoint = _editorView.GetCaretPosition(_editorBuffer);
            if (caretPoint == null) {
                return;
            }

            var snapshot = _editorBuffer.CurrentSnapshot;
            var lineNumber = snapshot.GetLineNumberFromPosition(caretPoint.Position);
            var line = snapshot.GetLineFromLineNumber(lineNumber + offset);
            var formatRange = new TextRange(line.Start, line.Length);

            UndoableFormatRange(formatRange);
        }

        public void UndoableFormatRange(ITextRange formatRange) {
            var es = _services.GetService<IEditorSupport>();
            using (var undoAction = es.CreateUndoAction(_editorView)) {
                undoAction.Open(Resources.AutoFormat);
                var formatter = new RangeFormatter(_services, _editorView, _editorBuffer, _changeHandler);
                var result = formatter.FormatRange(formatRange);
                if (result) {
                    undoAction.Commit();
                }
            }
        }

        public IAstNode GetIndentDefiningNode(AstRoot ast, int position) {
            var scope = ast.GetNodeOfTypeFromPosition<IScope>(position);
            // Scope indentation is defined by its parent statement.
            var parentStatement = scope.Parent as IAstNodeWithScope;
            if (parentStatement != null && parentStatement.Scope == scope) {
                return parentStatement;
            }
            return scope;
        }

        private void IndentCaretInNewScope(IScope scope, ISnapshotPoint caretBufferPoint, RFormatOptions options) {
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
            var positionInView = _editorView.MapToView(rTextBuffer.CurrentSnapshot, indentLine.Start);
            if (positionInView != null) {
                var viewIndentLine = _editorView.EditorBuffer.CurrentSnapshot.GetLineFromPosition(positionInView.Position);
                _editorView.Caret.MoveTo(viewIndentLine.Start, innerIndentSize);
            }
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Extensions;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Scopes;
using Microsoft.R.Core.AST.Statements;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Editor.SmartIndent;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting {
    internal static class FormatOperations {
        /// <summary>
        /// Formats statement that the caret is at
        /// </summary>
        public static void FormatCurrentStatement(ITextView textView, ITextBuffer textBuffer, IEditorShell editorShell, bool limitAtCaret = false, int caretOffset = 0) {
            SnapshotPoint? caretPoint = REditorDocument.MapCaretPositionFromView(textView);
            if (!caretPoint.HasValue) {
                return;
            }
            IREditorDocument document = REditorDocument.TryFromTextBuffer(textBuffer);
            if (document != null) {
                ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
                AstRoot ast = document.EditorTree.AstRoot;
                IAstNode node = ast.GetNodeOfTypeFromPosition<IStatement>(Math.Max(0, caretPoint.Value + caretOffset)) as IAstNode;
                FormatNode(textView, textBuffer, editorShell, node, limit: caretPoint.Value);
            }
        }

        /// <summary>
        /// Formats specific AST node 
        /// </summary>
        public static void FormatNode(ITextView textView, ITextBuffer textBuffer, IEditorShell editorShell, IAstNode node, int limit = -1) {
            if (node != null) {
                if (limit >= 0 && limit < node.Start) {
                    throw new ArgumentException(nameof(limit));
                }
                ITextRange range = limit < 0 ? node as ITextRange : TextRange.FromBounds(node.Start, limit);
                UndoableFormatRange(textView, textBuffer, range, editorShell);
            }
        }

        public static void FormatCurrentScope(ITextView textView, ITextBuffer textBuffer, IEditorShell editorShell, bool indentCaret) {
            // Figure out caret position in the document text buffer
            SnapshotPoint? caretPoint = REditorDocument.MapCaretPositionFromView(textView);
            if (!caretPoint.HasValue) {
                return;
            }
            IREditorDocument document = REditorDocument.TryFromTextBuffer(textBuffer);
            if (document != null) {
                // Make sure AST is up to date
                document.EditorTree.EnsureTreeReady();
                var ast = document.EditorTree.AstRoot;
                ITextSnapshot snapshot = textBuffer.CurrentSnapshot;

                // Find scope to format
                IScope scope = ast.GetNodeOfTypeFromPosition<IScope>(caretPoint.Value);

                using (var undoAction = editorShell.CreateCompoundAction(textView, textView.TextBuffer)) {
                    undoAction.Open(Resources.AutoFormat);
                    // Now format the scope
                    bool changed = RangeFormatter.FormatRange(textView, textBuffer, scope, REditorSettings.FormatOptions, editorShell);
                    if (indentCaret) {
                        // Formatting may change AST and the caret position so we need to reacquire both
                        caretPoint = REditorDocument.MapCaretPositionFromView(textView);
                        if (caretPoint.HasValue) {
                            document.EditorTree.EnsureTreeReady();
                            ast = document.EditorTree.AstRoot;
                            scope = ast.GetNodeOfTypeFromPosition<IScope>(caretPoint.Value);
                            IndentCaretInNewScope(textView, scope, caretPoint.Value, REditorSettings.FormatOptions);
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
        public static void FormatViewLine(ITextView textView, ITextBuffer textBuffer, int offset, IEditorShell editorShell) {
            SnapshotPoint? caretPoint = REditorDocument.MapCaretPositionFromView(textView);
            if (!caretPoint.HasValue) {
                return;
            }

            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            int lineNumber = snapshot.GetLineNumberFromPosition(caretPoint.Value.Position);
            ITextSnapshotLine line = snapshot.GetLineFromLineNumber(lineNumber + offset);
            ITextRange formatRange = new TextRange(line.Start, line.Length);

            UndoableFormatRange(textView, textBuffer, formatRange, editorShell, exactRange: true);
        }

        public static void UndoableFormatRange(ITextView textView, ITextBuffer textBuffer, ITextRange formatRange, IEditorShell editorShell, bool exactRange = false) {
            using (var undoAction = editorShell.CreateCompoundAction(textView, textView.TextBuffer)) {
                undoAction.Open(Resources.AutoFormat);
                var result = exactRange
                    ? RangeFormatter.FormatRangeExact(textView, textBuffer, formatRange, REditorSettings.FormatOptions, editorShell)
                    : RangeFormatter.FormatRange(textView, textBuffer, formatRange, REditorSettings.FormatOptions, editorShell);

                if (result) {
                    undoAction.Commit();
                }
            }
        }

        public static IAstNode GetIndentDefiningNode(AstRoot ast, int position) {
            IScope scope = ast.GetNodeOfTypeFromPosition<IScope>(position);
            // Scope indentation is defined by its parent statement.
            IAstNodeWithScope parentStatement = scope.Parent as IAstNodeWithScope;
            if (parentStatement != null && parentStatement.Scope == scope) {
                return parentStatement;
            }
            return scope;
        }

        private static void IndentCaretInNewScope(ITextView textView, IScope scope, SnapshotPoint caretBufferPoint, RFormatOptions options) {
            if (scope == null || scope.OpenCurlyBrace == null) {
                return;
            }
            ITextSnapshot rSnapshot = caretBufferPoint.Snapshot;
            ITextBuffer rTextBuffer = rSnapshot.TextBuffer;
            int rCaretPosition = caretBufferPoint.Position;

            var caretLine = rSnapshot.GetLineFromPosition(rCaretPosition);
            int innerIndentSize = SmartIndenter.InnerIndentSizeFromNode(rTextBuffer, scope, options);

            int openBraceLineNumber = rSnapshot.GetLineNumberFromPosition(scope.OpenCurlyBrace.Start);
            var braceLine = rSnapshot.GetLineFromLineNumber(openBraceLineNumber);
            var indentLine = rSnapshot.GetLineFromLineNumber(openBraceLineNumber + 1);

            string lineBreakText = braceLine.GetLineBreakText();
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

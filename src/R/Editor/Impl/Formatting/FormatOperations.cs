using System;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Text;
using Microsoft.Languages.Editor.Undo;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor.Document;
using Microsoft.R.Editor.Document.Definitions;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Editor.SmartIndent;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting {
    internal static class FormatOperations {
        /// <summary>
        /// Formats statement that the caret is at
        /// </summary>
        public static void FormatCurrentNode<T>(ITextView textView, ITextBuffer textBuffer) where T : class {
            SnapshotPoint? caretPoint = MapCaretToBuffer(textView, textBuffer);
            if (!caretPoint.HasValue) {
                return;
            }
            FormatNode<T>(textView, textBuffer, caretPoint.Value.Position);
        }

        /// <summary>
        /// Formats node at position
        /// </summary>
        public static void FormatNode<T>(ITextView textView, ITextBuffer textBuffer, int position) where T : class {
            IREditorDocument document = REditorDocument.TryFromTextBuffer(textBuffer);
            if (document != null) {
                ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
                AstRoot ast = document.EditorTree.AstRoot;
                IAstNode node = ast.GetNodeOfTypeFromPosition<T>(position) as IAstNode;
                UndoableFormatRange(textView, textBuffer, ast, node);
            }
        }

        public static void FormatCurrentScope(ITextView textView, ITextBuffer textBuffer, bool indentCaret) {
            SnapshotPoint? caretPoint = MapCaretToBuffer(textView, textBuffer);
            if (!caretPoint.HasValue) {
                return;
            }
            FormatScope(textView, textBuffer, caretPoint.Value, indentCaret);
        }

        public static void FormatScope(ITextView textView, ITextBuffer textBuffer, int position, bool indentCaret) {
            IREditorDocument document = REditorDocument.TryFromTextBuffer(textBuffer);
            if (document != null) {
                ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
                AstRoot ast = document.EditorTree.AstRoot;
                IScope scope = ast.GetNodeOfTypeFromPosition<IScope>(position);
                FormatScope(textView, textBuffer, ast, scope, indentCaret);
            }
        }

        /// <summary>
        /// Formats specific scope
        /// </summary>
        private static void FormatScope(ITextView textView, ITextBuffer textBuffer, AstRoot ast, IScope scope, bool indentCaret) {
            ICompoundUndoAction undoAction = EditorShell.Current.CreateCompoundAction(textView, textView.TextBuffer);
            undoAction.Open(Resources.AutoFormat);
            bool changed = false;

            try {
                // Now format the scope
                changed = RangeFormatter.FormatRange(textView, textBuffer, scope, ast, REditorSettings.FormatOptions);
                if (indentCaret) {
                    IndentCaretInNewScope(textView, textBuffer, scope, REditorSettings.FormatOptions);
                    changed = true;
                }
            } finally {
                undoAction.Close(!changed);
            }
        }

        /// <summary>
        /// Formats line relatively to the line that the caret is currently at
        /// </summary>
        public static void FormatLine(ITextView textView, ITextBuffer textBuffer, AstRoot ast, int offset) {
            SnapshotPoint? caretPoint = MapCaretToBuffer(textView, textBuffer);
            if (!caretPoint.HasValue) {
                return;
            }

            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            int lineNumber = snapshot.GetLineNumberFromPosition(caretPoint.Value.Position);
            ITextSnapshotLine line = snapshot.GetLineFromLineNumber(Math.Max(0, lineNumber + offset));
            ITextRange formatRange = new TextRange(line.Start, line.Length);

            UndoableFormatRange(textView, textBuffer, ast, formatRange);
        }

        public static void UndoableFormatRange(ITextView textView, ITextBuffer textBuffer, AstRoot ast, ITextRange formatRange) {
            ICompoundUndoAction undoAction = EditorShell.Current.CreateCompoundAction(textView, textView.TextBuffer);
            undoAction.Open(Resources.AutoFormat);
            bool changed = false;
            try {
                // Now format the scope
                changed = RangeFormatter.FormatRange(textView, textBuffer, formatRange, ast, REditorSettings.FormatOptions);
            } finally {
                undoAction.Close(!changed);
            }
        }

        private static SnapshotPoint? MapCaretToBuffer(ITextView textView, ITextBuffer textBuffer) {
            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            return textView.MapDownToBuffer(textView.Caret.Position.BufferPosition, textBuffer);
        }

        private static void IndentCaretInNewScope(ITextView textView, ITextBuffer textBuffer, IScope scope, RFormatOptions options) {
            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;

            SnapshotPoint? positionInBuffer = textView.MapDownToBuffer(textView.Caret.Position.BufferPosition, textBuffer);
            if (!positionInBuffer.HasValue) {
                return;
            }

            int position = positionInBuffer.Value.Position;
            ITextSnapshotLine caretLine = snapshot.GetLineFromPosition(position);

            int innerIndentSize = SmartIndenter.InnerIndentSizeFromScope(textBuffer, scope, options);

            int openBraceLineNumber = snapshot.GetLineNumberFromPosition(scope.OpenCurlyBrace.Start);
            ITextSnapshotLine braceLine = snapshot.GetLineFromLineNumber(openBraceLineNumber);
            ITextSnapshotLine indentLine = snapshot.GetLineFromLineNumber(openBraceLineNumber + 1);
            string lineBreakText = braceLine.GetLineBreakText();

            textBuffer.Insert(indentLine.Start, lineBreakText);

            positionInBuffer = textView.MapUpToBuffer(indentLine.Start.Position, textView.TextBuffer);
            if (!positionInBuffer.HasValue) {
                return;
            }

            indentLine = textView.TextBuffer.CurrentSnapshot.GetLineFromPosition(positionInBuffer.Value);
            textView.Caret.MoveTo(new VirtualSnapshotPoint(indentLine, innerIndentSize));
        }
    }
}

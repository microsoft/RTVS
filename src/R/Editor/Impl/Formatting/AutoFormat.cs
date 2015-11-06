using System;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Text;
using Microsoft.Languages.Editor.Undo;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Editor.SmartIndent;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting {
    internal static class AutoFormat {
        public static bool IsAutoformatTriggerCharacter(char ch) {
            return ch == '\n' || ch == '\r' || ch == ';';
        }

        public static bool IgnoreOnce { get; set; }

        public static void HandleType(ITextView textView, ITextBuffer textBuffer, AstRoot ast, char typedChar) {
            if (!REditorSettings.AutoFormat || textBuffer.CurrentSnapshot.Length == 0 || IgnoreOnce) {
                IgnoreOnce = false;
                return;
            }

            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            var positionInBuffer = textView.MapDownToBuffer(textView.Caret.Position.BufferPosition, textBuffer);
            if (positionInBuffer == null) {
                return;
            }

            int position = positionInBuffer.Value.Position;
            ITextSnapshotLine line = snapshot.GetLineFromPosition(position);
            ITextRange formatRange;

            if (ast.Comments.Contains(position)) {
                return;
            }

            IScope scope = ast.GetNodeOfTypeFromPosition<IScope>(position);
            if (typedChar == '}') {
                // If user typed } then format the enclosing scope
                scope = ast.GetNodeOfTypeFromPosition<IScope>(position - 1);
                FormatScope(textView, textBuffer, ast, scope, indentCaret: false);
            } else if (typedChar == '\n' || typedChar == '\r') {
                FormatLine(textView, textBuffer, ast, -1);
            } else {
                // Just format the line that was modified
                formatRange = new TextRange(position, 0);
                UndoableFormatRange(textView, textBuffer, ast, formatRange);
            }
        }

        /// <summary>
        /// Formats line relatively to the line that the caret is currently at
        /// </summary>
        public static void FormatLine(ITextView textView, ITextBuffer textBuffer, AstRoot ast, int offset) {
            if (!REditorSettings.AutoFormat || textBuffer.CurrentSnapshot.Length == 0 || IgnoreOnce) {
                IgnoreOnce = false;
                return;
            }

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

        /// <summary>
        /// Formats scope the caret is currently in
        /// </summary>
        public static void FormatCurrentScope(ITextView textView, ITextBuffer textBuffer, AstRoot ast, bool indentCaret) {
            if (!REditorSettings.AutoFormat || textBuffer.CurrentSnapshot.Length == 0) {
                return;
            }

            SnapshotPoint? caretPoint = MapCaretToBuffer(textView, textBuffer);
            if (!caretPoint.HasValue) {
                return;
            }

            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            IScope scope = ast.GetNodeOfTypeFromPosition<IScope>(caretPoint.Value.Position);

            FormatScope(textView, textBuffer, ast, scope, indentCaret);
        }

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

        private static void UndoableFormatRange(ITextView textView, ITextBuffer textBuffer, AstRoot ast, ITextRange formatRange) {
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
    }
}

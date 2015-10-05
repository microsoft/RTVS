using System;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Undo;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Editor.SmartIndent;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting
{
    internal static class AutoFormat
    {
        public static bool IsAutoformatTriggerCharacter(char ch)
        {
            return ch == '\n' || ch == '\r' || ch == '}' || ch == ';';
        }

        public static void HandleAutoFormat(ITextView textView, ITextBuffer textBuffer, AstRoot ast, char typedChar)
        {
            if (!REditorSettings.AutoFormat || textBuffer.CurrentSnapshot.Length == 0 || !IsAutoformatTriggerCharacter(typedChar))
            {
                return;
            }

            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            int position = textView.Caret.Position.BufferPosition;
            ITextSnapshotLine line = snapshot.GetLineFromPosition(position);
            ITextRange formatRange;

            if (ast.Comments.Contains(position))
            {
                return;
            }

            IScope scope = ast.GetNodeOfTypeFromPosition<IScope>(position);
            if (typedChar == '}')
            {
                // If user typed } then fromat the enclosing scope
                scope = ast.GetNodeOfTypeFromPosition<IScope>(position - 1);
                formatRange = scope;
            }
            else if (typedChar == '\n' || typedChar == '\r')
            {
                position = snapshot.GetLineFromLineNumber(line.LineNumber - 1).Start;
                formatRange = new TextRange(position, 0);
            }
            else
            {
                // Just format the line that was modified
                formatRange = new TextRange(position, 0);
            }

            ICompoundUndoAction undoAction = EditorShell.Current.CreateCompoundAction(textView, textView.TextBuffer);
            undoAction.Open(Resources.AutoFormat);
            bool changed = false;

            try
            {
                // Now format the scope
                changed = RangeFormatter.FormatRange(textView, formatRange, ast, REditorSettings.FormatOptions);

                // See if this was ENTER in {[whitespace]|[whitespace]} in which case
                // we want to add another line break and indent the caret so 
                //
                //      if (...) {
                //      |}
                //
                // turns into
                //
                //      if(...) {
                //          |
                //      }
                //
                // we do it AFTER formatting since it is when then indentation is known

                snapshot = textBuffer.CurrentSnapshot;
                position = textView.Caret.Position.BufferPosition;
                line = snapshot.GetLineFromPosition(position);

                string textBeforeCaret = snapshot.GetText(Span.FromBounds(line.Start, position));
                if (string.IsNullOrWhiteSpace(textBeforeCaret))
                {
                    string textAfterCaret = snapshot.GetText(Span.FromBounds(position, line.End)).TrimStart();
                    if (textAfterCaret.Length >= 1 && textAfterCaret[0] == '}' && scope != null && scope.OpenCurlyBrace != null)
                    {
                        // Open curly brace must be on the previous line
                        int openCurlyLineNumber = snapshot.GetLineNumberFromPosition(scope.OpenCurlyBrace.Start);
                        if (line.LineNumber == openCurlyLineNumber + 1)
                        {
                            IndentCaretInNewScope(textView, scope, REditorSettings.FormatOptions);
                            changed = true;
                        }
                    }
                }
            }
            finally
            {
                undoAction.Close(!changed);
            }
        }

        private static void IndentCaretInNewScope(ITextView textView, IScope scope, RFormatOptions options)
        {
            ITextBuffer textBuffer = textView.TextBuffer;
            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            int position = textView.Caret.Position.BufferPosition;
            ITextSnapshotLine caretLine = snapshot.GetLineFromPosition(position);

            int braceIndentSize = SmartIndenter.OuterIndentSizeFromScope(textBuffer, scope, options);
            string braceindentString = IndentBuilder.GetIndentString(braceIndentSize, options.IndentType, options.TabSize);

            int innerIndentSize = SmartIndenter.InnerIndentSizeFromScope(textBuffer, scope, options);
            string innerIndentString = IndentBuilder.GetIndentString(innerIndentSize, options.IndentType, options.TabSize);

            ITextSnapshotLine line = snapshot.GetLineFromPosition(scope.OpenCurlyBrace.Start);
            string lineBreakText = line.GetLineBreakText();

            textBuffer.Replace(Span.FromBounds(caretLine.Start, caretLine.End),
                innerIndentString + lineBreakText + braceindentString + "}");

            textView.Caret.MoveTo(new VirtualSnapshotPoint(textBuffer.CurrentSnapshot, caretLine.Start.Position + innerIndentString.Length));
        }
    }
}

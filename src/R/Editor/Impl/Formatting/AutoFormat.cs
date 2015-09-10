using System;
using Microsoft.Languages.Core.Formatting;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Undo;
using Microsoft.R.Core.AST;
using Microsoft.R.Core.AST.Definitions;
using Microsoft.R.Core.AST.Scopes.Definitions;
using Microsoft.R.Core.AST.Search;
using Microsoft.R.Core.Formatting;
using Microsoft.R.Editor.Settings;
using Microsoft.R.Editor.SmartIndent;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting
{
    internal static class AutoFormat
    {
        public static void HandleAutoFormat(ITextView textView, ITextBuffer textBuffer, AstRoot ast, char typedChar)
        {
            bool triggerCharacter = typedChar == '\n' || typedChar == '\r' || typedChar == '}' || typedChar == ';';
            if (!REditorSettings.AutoFormat || !triggerCharacter || textBuffer.CurrentSnapshot.Length == 0)
            {
                return;
            }

            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            int position = textView.Caret.Position.BufferPosition;
            ITextSnapshotLine line = snapshot.GetLineFromPosition(position);
            ITextRange formatRange;

            IScope scope = ast.GetNodeOfTypeFromPosition<IScope>(position);
            if (typedChar == '}' && scope != null && scope.OpenCurlyBrace != null)
            {
                // If user typed } then fromat the enclosing scope.
                formatRange = scope;
            }
            else if(typedChar == '\n' || typedChar == '\r')
            {
                position -= snapshot.GetLineFromPosition(line.LineNumber - 1).LineBreakLength;
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
                    if (textAfterCaret.Length == 1 && textAfterCaret[0] == '}' && scope != null)
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

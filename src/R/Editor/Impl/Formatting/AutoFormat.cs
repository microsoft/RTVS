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
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Formatting
{
    internal static class AutoFormat
    {
        public static void HandleAutoFormat(ITextView textView, ITextBuffer textBuffer, AstRoot ast)
        {
            if (!REditorSettings.AutoFormat || textBuffer.CurrentSnapshot.Length == 0)
                return;

            // Autoformatting takes time hence try not to format too much.
            // The file may be large and poorly formatted so when user types 
            // a semicolon or ENTER, the editor may spend good several seconds
            // getting poorly formatted code into shape. *Correct* search for 
            // the enclosing { } may also take fair amount of time since we'd 
            // need to tokenize the entire file. Note that opening and 
            // closing braces may be off screen so using classification
            // spans won't help either. 

            // Check for triggers. In R the triggers are ENTER,
            // closing curly brace or a semicolon (as opposed to C# or 
            // JS the latter is rare). Note that caret position at this
            // point is now after the typed trigger character so we need
            // to move back one character.

            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;
            int position = textView.Caret.Position.BufferPosition;
            if (position > 0)
            {
                position--;
            }

            char ch = snapshot[position];
            ITextRange formatRange = null;
            bool triggerCharacter = ch == '\n' || ch == '\r' || ch == '}' || ch == ';';

            if (ch == '\n' || ch == '\r')
            {
                // Line break may be two characters
                position--;
            }

            // If user typed } then fromat the nearest scope.
            // However, we can't use AST for this since the 
            // newly modified scope(s) haven't been parsed yet.
            // Since this is real-time formatting as user types
            // we can't be waiting for the parsing to complete.
            IScope scope = null;
            bool indentCaret = false;

            if (triggerCharacter)
            {
                int scopeStart = FindScopeStart(ast, position, out scope);
                if (scopeStart >= 0 && position >= scope.OpenCurlyBrace.End)
                {
                    string s = snapshot.GetText(Span.FromBounds(scope.OpenCurlyBrace.End, position));
                    indentCaret = string.IsNullOrWhiteSpace(s);

                    formatRange = new TextRange(scopeStart, position - scopeStart + 1);
                    if (formatRange == null)
                    {
                        // Just format the line that was modified
                        formatRange = new TextRange(position, 0);
                    }
                }
            }

            if (formatRange != null)
            {
                ICompoundUndoAction undoAction = EditorShell.CreateCompoundAction(textView, textView.TextBuffer);
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
                    // we do it AFTER formatting since now indentation is known

                    if (indentCaret && formatRange.Length > 0 && scope != null)
                    {
                        int caretPosition = textView.Caret.Position.BufferPosition;
                        ITextSnapshotLine line = textBuffer.CurrentSnapshot.GetLineFromPosition(caretPosition);
                        string lineText = line.GetText().Trim();

                        if (ch == '\n' || ch == '\r' || ch == '}')
                        {
                            indentCaret = lineText.Length == 1 && lineText[0] == '}';
                        }

                        if (indentCaret)
                        {
                            IndentCaretInNewScope(textView, position, scope, REditorSettings.FormatOptions);
                        }
                    }
                }
                finally
                {
                    undoAction.Close(!changed);
                }
            }
        }

        private static void IndentCaretInNewScope(ITextView textView, int position, IScope scope, RFormatOptions options)
        {
            ITextBuffer textBuffer = textView.TextBuffer;
            ITextSnapshot snapshot = textBuffer.CurrentSnapshot;

            // Position is before the freshly typed line break.
            // If we are here, some { } scope was found in the AST.
            int indentSize = RangeFormatter.IndentSizeFromScope(textBuffer, scope, options);
            ITextSnapshotLine line = snapshot.GetLineFromPosition(scope.OpenCurlyBrace.Start);

            string lineBreak = line.GetLineBreakText();
            string indentString = IndentBuilder.GetIndentString(indentSize, options.IndentType, options.TabSize);
            textBuffer.Insert(scope.OpenCurlyBrace.End, lineBreak + indentString);

            textView.Caret.MoveTo(new SnapshotPoint(textBuffer.CurrentSnapshot, scope.OpenCurlyBrace.End + lineBreak.Length + indentString.Length));
        }

        /// <summary>
        /// Given position of the closing curly brace in the buffer
        /// attempts to locate matching opening curly brace. The search
        /// is not perfect since it has to happen is a pretty short time.
        /// Out-of date AST is OK.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private static int FindScopeStart(AstRoot ast, int position, out IScope scope)
        {
            scope = ast.GetSpecificNodeFromPosition(position, (IAstNode n) =>
            {
                return n is IScope;
            }) as IScope;

            if (scope != null && scope.OpenCurlyBrace != null)
            {
                return scope.OpenCurlyBrace.Start;
            }

            scope = null;
            return -1;
        }
    }
}

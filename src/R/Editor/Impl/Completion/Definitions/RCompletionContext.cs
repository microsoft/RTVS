using System;
using Microsoft.R.Core.AST;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Completion.Definitions
{
    /// <summary>
    /// R completion context. Provides information about current document, 
    /// caret position and other necessary data for the completion engine.
    /// </summary>
    public class RCompletionContext
    {
        public int Position { get; set; }
        public ICompletionSession Session { get; private set; }
        public AstRoot AstRoot { get; private set; }
        public bool InternalFunctions { get; internal set; }

        public RCompletionContext(ICompletionSession session, AstRoot ast, int position)
        {
            Session = session;
            Position = position;
            AstRoot = ast;
        }

        public bool IsInNameSpace()
        {
            return IsInNamespace(Session.TextView);
        }

        public static bool IsInNamespace(ITextView textView)
        {
            try
            {
                int caretPosition = textView.Caret.Position.BufferPosition;
                ITextSnapshot snapshot = textView.TextBuffer.CurrentSnapshot;
                ITextSnapshotLine line = snapshot.GetLineFromPosition(caretPosition);
                if (line.Length > 2 && caretPosition - line.Start > 2)
                {
                    return snapshot[caretPosition - 1] == ':';
                }
            }
            catch (Exception) { }

            return false;
        }

        public static bool IsInLibraryStatement(ITextView textView)
        {
            try
            {
                int caretPosition = textView.Caret.Position.BufferPosition;
                ITextSnapshot snapshot = textView.TextBuffer.CurrentSnapshot;
                ITextSnapshotLine line = snapshot.GetLineFromPosition(caretPosition);

                if (line.Length < 8 || caretPosition < line.Start + 8 || snapshot[caretPosition - 1] != '(')
                {
                    return false;
                }

                int start = -1;
                int end = -1;
 
                for (int i = caretPosition - 2; i >= 0; i--)
                {
                    if (!char.IsWhiteSpace(snapshot[i]))
                    {
                        end = i + 1;
                        break;
                    }
                }

                if (end <= 0)
                {
                    return false;
                }

                for (int i = end - 1; i >= 0; i--)
                {
                    if (char.IsWhiteSpace(snapshot[i]))
                    {
                        start = i + 1;
                        break;
                    }
                    else if(i == 0)
                    {
                        start = 0;
                        break;
                    }
                }

                if (start < 0 || end <= start)
                {
                    return false;
                }

                start -= line.Start;
                end -= line.Start;

                string s = line.GetText().Substring(start, end - start);
                if (s == "library" || s == "require")
                {
                    return true;
                }
            }
            catch (Exception) { }

            return false;
        }
    }
}

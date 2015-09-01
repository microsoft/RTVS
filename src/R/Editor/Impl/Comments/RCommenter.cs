using System;
using Microsoft.Languages.Core.Text;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Undo;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Comments
{
    /// <summary>
    /// Provides functionality for comment/uncomment commands
    /// </summary>
    public static class RCommenter
    {
        /// <summary>
        /// Comments selected lines or current line if range has zero length.
        /// Continues adding commentcharacter even if line is already commented.
        /// # -> ## -> ### and so on. Matches C# behavior.
        /// </summary>
        public static void CommentBlock(ITextView textView, ITextBuffer textBuffer, ITextRange range)
        {
            DoActionOnLines(textView, textBuffer, range, CommentLine, Resources.CommentSelection);
        }

        /// <summary>
        /// Uncomments selected lines or current line if range has zero length.
        /// Only removes single comment. ### -> ## -> # and so on. Matches C# behavior.
        /// </summary>
        public static void UncommentBlock(ITextView textView, ITextBuffer textBuffer, ITextRange range)
        {
            DoActionOnLines(textView, textBuffer, range, UncommentLine, Resources.UncommentSelection);
        }

        public static void DoActionOnLines(ITextView textView, ITextBuffer textBuffer, ITextRange range, Func<ITextSnapshotLine, bool> action, string actionName)
        {
            int startLineNumber = textBuffer.CurrentSnapshot.GetLineNumberFromPosition(range.Start);
            int endLineNumber = textBuffer.CurrentSnapshot.GetLineNumberFromPosition(range.End);

            ICompoundUndoAction undoAction = EditorShell.CreateCompoundAction(textView, textBuffer);
            undoAction.Open(actionName);
            bool changed = false;
            try
            {
                for (int i = startLineNumber; i <= endLineNumber; i++)
                {
                    ITextSnapshotLine line = textBuffer.CurrentSnapshot.GetLineFromLineNumber(i);
                    changed |= action(line);
                }
            }
            finally
            {
                undoAction.Close(!changed);
            }
        }

        internal static bool CommentLine(ITextSnapshotLine line)
        {
            string lineText = line.GetText();
            if (!string.IsNullOrWhiteSpace(lineText))
            {
                int leadingWsLength = lineText.Length - lineText.TrimStart().Length;
                line.Snapshot.TextBuffer.Insert(line.Start + leadingWsLength, "#");
                return true;
            }

            return false;
        }

        internal static bool UncommentLine(ITextSnapshotLine line)
        {
            string lineText = line.GetText();
            if (!string.IsNullOrWhiteSpace(lineText))
            {
                int leadingWsLength = lineText.Length - lineText.TrimStart().Length;
                if (leadingWsLength < lineText.Length)
                {
                    if (lineText[leadingWsLength] == '#')
                    {
                        line.Snapshot.TextBuffer.Delete(new Span(line.Start + leadingWsLength, 1));
                        return true;
                    }
                }
            }

            return false;
        }
    }
}

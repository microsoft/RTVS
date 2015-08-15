using System;
using Microsoft.VisualStudio.Text;
using Microsoft.Languages.Editor.Selection;
using Microsoft.Languages.Editor.EditorHelpers;

namespace Microsoft.Languages.Editor.Text
{
    public static class IncrementalTextChangeApplication
    {
        // This function takes current text buffer and newly text, builds 
        // a list of changed regions and applies them to the buffer.
        // This way we can avoid destruction of bookmarks and other markers. Complete
        // buffer replacement deletes all markers which causes loss of bookmarks.
        // Note that HTML variant in venus\html\html\... (TextBufferWrapper.cs) 
        // also walks through markers and moves them around, but it is about O(n^2)
        // so I am reluctant to use it. Simple list of changes seem to work for CSS
        // in majority of cases. List of tracking positions helps with selection
        // and caret position preservation on formatting.

        public static void ApplyChange(
            ITextBuffer textBuffer,
            int position,
            int length,
            string newText,
            string transactionName,
            ISelectionTracker selectionTracker,
            int maxMilliseconds,
            Func<char, bool> isDelimiter)
        {
            var snapshot = textBuffer.CurrentSnapshot;
            int oldLength = Math.Min(length, snapshot.Length - position);
            string oldText = snapshot.GetText(position, oldLength);

            var changes = TextChanges.BuildChangeList(oldText, newText, maxMilliseconds, isDelimiter);
            if (changes != null && changes.Count > 0)
            {
                using (var selectionUndo = new SelectionUndo(selectionTracker, transactionName, automaticTracking: false))
                {
                    using (ITextEdit edit = textBuffer.CreateEdit())
                    {
                        // Replace ranges in reverse so relative positions match
                        for (int i = changes.Count - 1; i >= 0; i--)
                        {
                            TextChange tc = changes[i];
                            edit.Replace(tc.Position + position, tc.Length, tc.NewText);
                        }

                        edit.Apply();
                    }
                }
            }
        }
    }
}

using System;
using Microsoft.Languages.Editor.EditorHelpers;
using Microsoft.Languages.Editor.Selection;
using Microsoft.VisualStudio.Text;

namespace Microsoft.Languages.Editor.Text
{
    public static class IncrementalTextChangeApplication
    {
        /// Takes current text buffer and new text then builds list of changed
        /// regions and applies them to the buffer. This way we can avoid 
        /// destruction of bookmarks and other markers. Complete
        /// buffer replacement deletes all markers which causes 
        /// loss of bookmarks, breakpoints and other similar markers.

        public static void ApplyChange(
            ITextBuffer textBuffer,
            int position,
            int length,
            string newText,
            string transactionName,
            ISelectionTracker selectionTracker,
            int maxMilliseconds)
        {
            var snapshot = textBuffer.CurrentSnapshot;
            int oldLength = Math.Min(length, snapshot.Length - position);
            string oldText = snapshot.GetText(position, oldLength);

            var changes = TextChanges.BuildChangeList(oldText, newText, maxMilliseconds);
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

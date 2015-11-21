using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;

namespace Microsoft.VisualStudio.R.Package.History {
    public interface IRHistory {
        event EventHandler<EventArgs> SelectionChanged;

        bool TryLoadFromFile(string path);
        bool TrySaveToFile(string path);
        void SendSelectedToRepl();
        void SendSelectedToTextView(IWpfTextView textView);

        IList<SnapshotSpan> GetSelectedHistoryEntrySpans();
        string GetSelectedText();
        SnapshotSpan SelectHistoryEntry(int lineNumber);
        SnapshotSpan DeselectHistoryEntry(int lineNumber);
        SnapshotSpan ToggleHistoryEntrySelection(int lineNumber);
        void ClearHistoryEntrySelection();
        void DeleteSelectedHistoryEntries();

        void AddToHistory(string text);
    }
}
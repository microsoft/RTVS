using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.History {
    public interface IRHistory {
        event EventHandler<EventArgs> SelectionChanged;
        bool HasSelectedEntries { get; }
        bool HasEntries { get; }

        bool TryLoadFromFile(string path);
        bool TrySaveToFile(string path);
        void SendSelectedToRepl();
        void SendSelectedToTextView(IWpfTextView textView);
        void CopySelection();

        IList<SnapshotSpan> GetSelectedHistoryEntrySpans();
        string GetSelectedText();
        SnapshotSpan SelectHistoryEntry(int lineNumber);
        SnapshotSpan DeselectHistoryEntry(int lineNumber);
        SnapshotSpan ToggleHistoryEntrySelection(int lineNumber);
        void SelectAllEntries();
        void ClearHistoryEntrySelection();
        void DeleteSelectedHistoryEntries();
        void DeleteAllHistoryEntries();

        void AddToHistory(string text);
    }
}
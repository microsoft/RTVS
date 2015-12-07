using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.R.Package.History {
    public interface IRHistory {
        event EventHandler<EventArgs> SelectionChanged;
        event EventHandler<EventArgs> HistoryChanged;
        bool HasSelectedEntries { get; }
        bool HasEntries { get; }

        bool TryLoadFromFile(string path);
        bool TrySaveToFile(string path);
        void SendSelectedToRepl();
        void SendSelectedToTextView(ITextView textView);
        void PreviousEntry();
        void NextEntry();
        void CopySelection();

        IReadOnlyList<SnapshotSpan> GetSelectedHistoryEntrySpans();
        string GetSelectedText();
        SnapshotSpan SelectHistoryEntry(int lineNumber);
        SnapshotSpan DeselectHistoryEntry(int lineNumber);
        SnapshotSpan ToggleHistoryEntrySelection(int lineNumber);

        void SelectHistoryEntries(IEnumerable<int> lineNumbers);
        void SelectAllEntries();
        void ClearHistoryEntrySelection();
        void DeleteSelectedHistoryEntries();
        void DeleteAllHistoryEntries();
        void Filter(string searchPattern);
        void ClearFilter();

        void AddToHistory(string text);
    }
}
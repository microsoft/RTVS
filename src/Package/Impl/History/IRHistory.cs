using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Projection;

namespace Microsoft.VisualStudio.R.Package.History {
    public interface IRHistory : IDisposable {
        IWpfTextView GetOrCreateTextView();

        event EventHandler<EventArgs> SelectionChanged;
        event EventHandler<EventArgs> HistoryChanged;
        bool HasSelectedEntries { get; }
        bool HasEntries { get; }
        bool IsMultiline { get; set; }

        bool TryLoadFromFile(string path);
        bool TrySaveToFile(string path);
        void SendSelectedToRepl();
        void SendSelectedToTextView(ITextView textView);
        void PreviousEntry();
        void NextEntry();
        void CopySelection();

        IReadOnlyList<SnapshotSpan> GetAllHistoryEntrySpans();
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

        void AddToHistory(string text);

        void Workaround169159(IElisionBuffer elisionBuffer);
    }
}
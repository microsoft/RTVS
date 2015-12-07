using System;
using System.Collections.Generic;

namespace Microsoft.VisualStudio.R.Package.History {
    internal interface IRHistoryEntries {
        IReadOnlyList<IRHistoryEntry> GetEntries();
        IReadOnlyList<IRHistoryEntry> GetSelectedEntries();
        IEnumerable<string> GetEntriesText();
        IEnumerable<string> GetSelectedEntriesText();
        IRHistoryEntry Find(Func<IRHistoryEntry, bool> predicate);
        IRHistoryEntry FirstOrDefault();
        IRHistoryEntry LastOrDefault();
        bool HasEntries { get; }
        bool HasSelectedEntries { get; }
        IRHistoryEntry Add(string text);
        void Remove(IRHistoryEntry historyEntry);
        void SelectAll();
        void UnselectAll();
        void RemoveSelected();
        void RemoveAll();
    }
}
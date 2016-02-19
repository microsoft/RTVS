using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Components.History {
    internal interface IRHistoryEntries {
        IReadOnlyList<IRHistoryEntry> GetEntries();
        IReadOnlyList<IRHistoryEntry> GetSelectedEntries();
        IRHistoryEntry Find(Func<IRHistoryEntry, bool> predicate);
        IRHistoryEntry FirstOrDefault();
        IRHistoryEntry LastOrDefault();
        bool IsMultiline { get; }
        bool HasEntries { get; }
        bool HasSelectedEntries { get; }
        void Add(ITrackingSpan entrySpan);
        void Remove(IRHistoryEntry historyEntry);
        void SelectAll();
        void UnselectAll();
        void RemoveSelected();
        void RemoveAll();
    }
}
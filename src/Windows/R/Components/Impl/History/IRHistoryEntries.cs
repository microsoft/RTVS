// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Components.History {
    internal interface IRHistoryEntries {
        IReadOnlyList<IRHistoryEntry> GetEntries();
        IReadOnlyList<IRHistoryEntry> GetSelectedEntries();
        IRHistoryEntry FindEntryContainingSpan(SnapshotSpan span, ITextSnapshot snapshot);
        IRHistoryEntry FindEntryContainingPoint(SnapshotPoint point, ITextSnapshot snapshot);
        int FindEntryIndexContainingSpan(SnapshotSpan span, ITextSnapshot snapshot);
        IRHistoryEntry First();
        IRHistoryEntry Last();
        IRHistoryEntry LastSelected();
        int LastSelectedIndex();
        int Count { get; }
        bool IsMultiline { get; }
        bool HasSelectedEntries { get; }
        void Add(ITrackingSpan entrySpan);
        void SelectRangeTo(int rangeEndIndex);
        void SelectAll();
        void UnselectAll();
        void RemoveSelected();
        void RemoveAll();
    }
}
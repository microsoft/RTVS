// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Components.History {
    internal interface IRHistoryEntries {
        IReadOnlyList<IRHistoryEntry> GetEntries();
        IReadOnlyList<IRHistoryEntry> GetSelectedEntries();
        IRHistoryEntry Find(Func<IRHistoryEntry, bool> predicate);
        IRHistoryEntry First();
        IRHistoryEntry Last();
        IRHistoryEntry LastSelected();
        bool IsMultiline { get; }
        bool HasEntries { get; }
        bool HasSelectedEntries { get; }
        void Add(ITrackingSpan entrySpan);
        void SelectRangeTo(IRHistoryEntry entry);
        void SelectAll();
        void UnselectAll();
        void RemoveSelected();
        void RemoveAll();
    }
}
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Components.History {
    public interface IRHistory : IDisposable {
        IRHistoryWindowVisualComponent GetOrCreateVisualComponent(IRHistoryVisualComponentContainerFactory componentContainerFactory, int instanceId = 0);

        event EventHandler<EventArgs> SelectionChanged;
        event EventHandler<EventArgs> HistoryChanging;
        event EventHandler<EventArgs> HistoryChanged;

        IRHistoryWindowVisualComponent VisualComponent { get; }

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

        void ScrollToTop();
        void ScrollPageUp();
        void ScrollPageDown();
        void ScrollToBottom();

        IReadOnlyList<SnapshotSpan> GetAllHistoryEntrySpans();
        IReadOnlyList<SnapshotSpan> GetSelectedHistoryEntrySpans();
        string GetSelectedText();

        SnapshotSpan SelectHistoryEntry(int lineNumber);
        SnapshotSpan DeselectHistoryEntry(int lineNumber);
        SnapshotSpan ToggleHistoryEntrySelection(int lineNumber);
        void SelectNextHistoryEntry();
        void SelectPreviousHistoryEntry();
        void ToggleHistoryEntriesRangeSelectionUp();
        void ToggleHistoryEntriesRangeSelectionDown();
        void ToggleTextSelectionLeft();
        void ToggleTextSelectionRight();
        void SelectHistoryEntriesRangeTo(int lineNumber);
        void SelectAllEntries();
        void ClearHistoryEntrySelection();

        void DeleteSelectedHistoryEntries();
        void DeleteAllHistoryEntries();

        void AddToHistory(string text);
    }
}
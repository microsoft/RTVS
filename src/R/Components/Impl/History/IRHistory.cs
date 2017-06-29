// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.R.Components.History {
    public interface IRHistory : IDisposable {
        event EventHandler<EventArgs> SelectionChanged;
        event EventHandler<EventArgs> HistoryChanging;
        event EventHandler<EventArgs> HistoryChanged;

        bool HasSelectedEntries { get; }
        bool HasEntries { get; }
        bool IsMultiline { get; set; }

        bool TryLoadFromFile(string path);
        bool TrySaveToFile(string path);
        void SendSelectedToRepl();
        void PreviousEntry();
        void NextEntry();
        void CopySelection();

        void ScrollToTop();
        void ScrollPageUp();
        void ScrollPageDown();
        void ScrollToBottom();

        string GetSelectedText();
        IReadOnlyList<string> Search(string entryStart);

        void SelectHistoryEntry(int lineNumber);
        void DeselectHistoryEntry(int lineNumber);
        void ToggleHistoryEntrySelection(int lineNumber);
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
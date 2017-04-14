// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Components.History.Implementation {
    internal sealed partial class RHistory {
        private sealed class SingleEntrySelector : IEntrySelector {
            private readonly RHistory _history;
            private readonly bool _isReversed;

            public SingleEntrySelector(RHistory history, bool isReversed) {
                _history = history;
                _isReversed = isReversed;
            }

            public void EntriesSelected() {
                var entryToSelect = GetEntryAfterSelected();
                if (entryToSelect == null) {
                    return;
                }

                _history.ClearHistoryEntrySelection();
                SelectAndDisplayEntry(entryToSelect);
            }

            public void TextSelected() {
                var selection = _history.VisualComponent.TextView.Selection;
                IRHistoryEntry entryToSelect;
                if (selection.IsEmpty) {
                    entryToSelect = GetEntryAtEnd();
                } else {
                    entryToSelect = GetEntryToSelectFromPosition(selection);
                    if (entryToSelect == null) {
                        return;
                    }

                    selection.Clear();
                }

                SelectAndDisplayEntry(entryToSelect);
            }

            private IRHistoryEntry GetEntryAfterSelected() => _isReversed
                ? _history._entries.LastSelected().Previous
                : _history._entries.LastSelected().Next;

            private IRHistoryEntry GetEntryAtEnd() => _isReversed 
                ? _history._entries.Last() 
                : _history._entries.First();

            private IRHistoryEntry GetEntryToSelectFromPosition(ITextSelection selection) => _isReversed 
                ? _history.GetHistoryEntryFromPosition(selection.Start.Position).Previous
                : _history.GetHistoryEntryFromPosition(selection.End.Position).Next;

            private void SelectAndDisplayEntry(IRHistoryEntry entry) => 
                _history.SelectAndDisplayEntry(entry, _isReversed ? ViewRelativePosition.Top : ViewRelativePosition.Bottom);
        }
    }
}
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Linq;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Components.History.Implementation {
    internal sealed class MultilineRHistoryEntries : RHistoryEntries {
        public override bool IsMultiline => true;

        public MultilineRHistoryEntries() {
        }

        public MultilineRHistoryEntries(IRHistoryEntries entries) {
            if (!entries.IsMultiline) {
                CloneFromSingleline(entries);
            } else {
                CloneEntries(entries);
            }
        }

        public override void Add(ITrackingSpan entrySpan) {
            AddMultilineEntry(entrySpan);
        }

        private void CloneFromSingleline(IRHistoryEntries existingEntries) {
            foreach (var entrySpanGroup in existingEntries.GetEntries().GroupBy(e => e.EntrySpan)) {
                var newEntry = AddMultilineEntry(entrySpanGroup.Key);
                newEntry.IsSelected = entrySpanGroup.Any(e => e.IsSelected);
            }
        }

        private IRHistoryEntry AddMultilineEntry(ITrackingSpan entrySpan) {
            return AddEntry(entrySpan, entrySpan);
        }
    }
}
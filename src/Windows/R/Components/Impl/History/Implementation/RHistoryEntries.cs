// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Collections;
using Microsoft.VisualStudio.Text;

namespace Microsoft.R.Components.History.Implementation {
    internal abstract class RHistoryEntries : IRHistoryEntries {
        private static readonly IComparer<Entry> EntryComparer = Comparer<Entry>.Create((e1, e2) => e1.Index.CompareTo(e2.Index));

        private long _index;
        private bool _isRangeSelected;
        private readonly List<Entry> _entries = new List<Entry>();
        private readonly List<Entry> _selectedStack = new List<Entry>();

        public abstract bool IsMultiline { get; }

        public IReadOnlyList<IRHistoryEntry> GetEntries() => new List<IRHistoryEntry>(_entries);
        public IReadOnlyList<IRHistoryEntry> GetSelectedEntries() {
            var sortedList = new List<Entry>(_selectedStack);
            sortedList.Sort(EntryComparer);
            return sortedList;
        }

        public IRHistoryEntry FindEntryContainingSpan(SnapshotSpan span, ITextSnapshot snapshot) => 
            _entries.First(e => e.Span?.GetSpan(snapshot).Contains(span) == true);
        public IRHistoryEntry FindEntryContainingPoint(SnapshotPoint point, ITextSnapshot snapshot) => 
            _entries.First(e => e.Span?.GetSpan(snapshot).Contains(point) == true);
        public int FindEntryIndexContainingSpan(SnapshotSpan span, ITextSnapshot snapshot) =>
            _entries.IndexWhere(e => e.Span?.GetSpan(snapshot).Contains(span) == true).First();

        public IRHistoryEntry First() => _entries.First();
        public IRHistoryEntry Last() => _entries.Last();
        public IRHistoryEntry LastSelected() => _selectedStack[_selectedStack.Count - 1];
        public int LastSelectedIndex() => _entries.BinarySearch(_selectedStack[_selectedStack.Count - 1], EntryComparer);
        public int Count => _entries.Count;
        public bool HasSelectedEntries => _selectedStack.Count > 0;

        public abstract void Add(ITrackingSpan span);

        protected IRHistoryEntry AddEntry(ITrackingSpan entrySpan, ITrackingSpan span) {
            var entry = new Entry(this, _index++, entrySpan, span);

            if (_entries.Count > 0) {
                var previous = _entries[_entries.Count - 1];
                entry.Previous = previous;
                previous.Next = entry;
            }

            _entries.Add(entry);
            return entry;
        }

        public void SelectRangeTo(int rangeEndIndex) {
            if (rangeEndIndex < 0 || rangeEndIndex >= Count) {
                throw new ArgumentOutOfRangeException(nameof(rangeEndIndex));
            }

            var rangeStart = _isRangeSelected ? _selectedStack[0] : _selectedStack[_selectedStack.Count - 1];
            var rangeStartIndex = _entries.BinarySearch(rangeStart, EntryComparer);
            Entry[] range;
            if (rangeStartIndex < rangeEndIndex) {
                range = new Entry[rangeEndIndex - rangeStartIndex + 1];
                _entries.CopyTo(rangeStartIndex, range, 0, range.Length);
            } else {
                range = new Entry[rangeStartIndex - rangeEndIndex + 1];
                _entries.CopyTo(rangeEndIndex, range, 0, range.Length);
                Array.Reverse(range);
            }
 
            SelectRange(range);
        }

        public void SelectAll() {
            SelectRange(_entries);
        }

        public void UnselectAll() {
            ClearSelection();
        }

        public void RemoveSelected() {
            foreach (var entry in _selectedStack) {
                DeleteEntry(entry);
            }

            ClearSelection();
        }

        public void RemoveAll() {
            foreach (var entry in _entries) {
                entry.Dispose();
            }

            _entries.Clear();
            ClearSelection();
            _index = 0;
        }

        protected void CloneEntries(IRHistoryEntries existingEntries) {
            foreach (var existingEntry in existingEntries.GetEntries()) {
                var newEntry = AddEntry(existingEntry.EntrySpan, existingEntry.Span);
                newEntry.IsSelected = existingEntry.IsSelected;
            }
        }

        private void DeleteEntry(Entry entry) {
            _entries.RemoveSorted(entry, EntryComparer);
            var previous = (Entry)entry.Previous;
            var next = (Entry)entry.Next;
            if (previous != null) {
                previous.Next = next;
            }
            if (next != null) {
                next.Previous = previous;
            }
            entry.Dispose();
        }

        private void ChangeSelection(Entry entry, bool isSelected) {
            if (isSelected) {
                _selectedStack.Add(entry);
            } else {
                _selectedStack.Remove(entry);
            }

            _isRangeSelected = false;
        }
        
        private void SelectRange(IEnumerable<Entry> entries) {
            foreach (var entry in _selectedStack) {
                entry.MarkUnselected();
            }

            _selectedStack.Clear();
            _selectedStack.AddRange(entries);

            foreach (var entry in _selectedStack) {
                entry.MarkSelected();
            }
            _isRangeSelected = true;
        }

        private void ClearSelection() {
            foreach (var entry in _selectedStack) {
                entry.MarkUnselected();
            }
            _selectedStack.Clear();

            _isRangeSelected = false;
        }

        private class Entry : IRHistoryEntry, IDisposable {
            private bool _isSelected;

            public Entry(RHistoryEntries owner, long index, ITrackingSpan entrySpan, ITrackingSpan span) {
                Owner = owner;
                Index = index;
                EntrySpan = entrySpan;
                Span = span;
            }

            public long Index { get; }
            public ITrackingSpan EntrySpan { get; private set; }
            public ITrackingSpan Span { get; private set; }
            public RHistoryEntries Owner { get; private set; }

            public IRHistoryEntry Next { get; set; }
            public IRHistoryEntry Previous { get; set; }

            public bool IsSelected {
                get { return _isSelected; }
                set {
                    if (value != _isSelected) {
                        Owner.ChangeSelection(this, value);
                    }

                    _isSelected = value;
                }
            }

            public void MarkSelected() {
                _isSelected = true;
            }

            public void MarkUnselected() {
                _isSelected = false;
            }

            public void Dispose() {
                EntrySpan = null;
                Span = null;
                Owner = null;
                Previous = null;
                Next = null;
            }
        }
    }
}
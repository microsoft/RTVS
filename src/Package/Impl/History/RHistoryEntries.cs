using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Languages.Core.Utility;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.R.Package.History {
    internal abstract class RHistoryEntries : IRHistoryEntries {
        private static readonly IComparer<Entry> EntryComparer = Comparer<Entry>.Create((e1, e2) => e1.Index.CompareTo(e2.Index));

        private long _index;
        private readonly List<Entry> _entries = new List<Entry>();
        private List<Entry> _selectedEntries = new List<Entry>();

        public abstract bool IsMultiline { get; }

        public IReadOnlyList<IRHistoryEntry> GetEntries() => new List<IRHistoryEntry>(_entries);
        public IReadOnlyList<IRHistoryEntry> GetSelectedEntries() => new List<IRHistoryEntry>(_selectedEntries);
        public IRHistoryEntry Find(Func<IRHistoryEntry, bool> predicate) => _entries.First(predicate);
        public IRHistoryEntry FirstOrDefault() => _entries.FirstOrDefault();
        public IRHistoryEntry LastOrDefault() => _entries.LastOrDefault();
        public bool HasEntries => _entries.Count > 0;
        public bool HasSelectedEntries => _selectedEntries.Count > 0;

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

        public void Remove(IRHistoryEntry historyEntry) {
            var entry = historyEntry as Entry;
            if (entry == null || entry.Owner != this) {
                throw new ArgumentException(nameof(historyEntry));
            }

            _selectedEntries.RemoveSorted(entry, EntryComparer);
            DeleteEntry(entry);
        }

        public void SelectAll() {
            _selectedEntries = new List<Entry>(_entries);

            foreach (var entry in _selectedEntries) {
                entry.MarkSelected();
            }
        }

        public void UnselectAll() {
            var selectedEntries = _selectedEntries;
            _selectedEntries = new List<Entry>();

            foreach (var entry in selectedEntries) {
                entry.IsSelected = false;
            }
        }

        public void RemoveSelected() {
            var selectedEntries = _selectedEntries;
            _selectedEntries = new List<Entry>();

            foreach (var entry in selectedEntries) {
                DeleteEntry(entry);
            }
        }

        public void RemoveAll() {
            foreach (var entry in _entries) {
                entry.Dispose();
            }

            _entries.Clear();
            _selectedEntries.Clear();
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
                _selectedEntries.AddSorted(entry, EntryComparer);
            } else {
                _selectedEntries.RemoveSorted(entry, EntryComparer);
            }
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
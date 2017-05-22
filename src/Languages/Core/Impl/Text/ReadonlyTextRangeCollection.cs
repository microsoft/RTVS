// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Languages.Core.Text {
    public sealed class ReadOnlyTextRangeCollection<T> : IReadOnlyTextRangeCollection<T>, IEnumerable<T>, IEnumerable where T : ITextRange {
        private readonly TextRangeCollection<T> _collection;

        [DebuggerStepThrough]
        public ReadOnlyTextRangeCollection(TextRangeCollection<T> collection) {
            _collection = collection;
        }

        public int Start => _collection.Start;
        public int End => _collection.End;
        public int Length => _collection.Length;
        public bool Contains(int position) => _collection.Contains(position);

        public void Shift(int offset) => _collection.Shift(offset);

        public void ShiftStartingFrom(int position, int offset) => _collection.ShiftStartingFrom(position, offset);
        public IReadOnlyList<T> ItemsInRange(ITextRange range) => _collection.ItemsInRange(range);
        public IReadOnlyList<T> ItemsInRange(int start) => _collection.ItemsInRange(TextRange.FromBounds(start, start));

        public IReadOnlyList<int> GetItemsContainingInclusiveEnd(int position) 
            => _collection.GetItemsContainingInclusiveEnd(position);

        public int Count => _collection.Count;

        /// <summary>
        /// Retrieves Nth item in the collection
        /// </summary>
        public T this[int index] => _collection[index];

        /// <summary>
        /// Returns index of item that starts at the given position if exists, -1 otherwise.
        /// </summary>
        /// <param name="position">Position in a text buffer</param>
        /// <returns>Item index or -1 if not found</returns>
        public int GetItemAtPosition(int position) => _collection.GetItemAtPosition(position);
        public int GetItemContaining(int position) => _collection.GetItemContaining(position);
        public int GetFirstItemBeforePosition(int position) => _collection.GetFirstItemBeforePosition(position);
        public int GetFirstItemAfterOrAtPosition(int position) => _collection.GetFirstItemAfterOrAtPosition(position);

        public T[] ToArray() => _collection.ToArray();

        #region IEnumerable<T> Members
        public IEnumerator<T> GetEnumerator() => _collection.GetEnumerator();
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator() => _collection.GetEnumerator();
        #endregion
    }
}

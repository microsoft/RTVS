using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Languages.Core.Text
{
    public sealed class ReadOnlyTextRangeCollection<T> : IReadOnlyTextRangeCollection<T>, IEnumerable<T>, IEnumerable where T: ITextRange
    {
        private TextRangeCollection<T> _collection;

        public ReadOnlyTextRangeCollection(TextRangeCollection<T> collection)
        {
            _collection = collection;
        }

        public int Start { get { return _collection.Start; } }
        public int End { get { return _collection.End; } }

        public int Length { get { return _collection.Length; } }

        public bool Contains(int position) { return _collection.Contains(position); }

        public void Shift(int offset)
        {
            _collection.Shift(offset);
        }

        public void ShiftStartingFrom(int position, int offset)
        {
            _collection.ShiftStartingFrom(position, offset);
        }

        public IReadOnlyList<T> ItemsInRange(ITextRange range)
        {
            return _collection.ItemsInRange(range);
        }

        public IReadOnlyList<T> ItemsInRange(int start)
        {
            return _collection.ItemsInRange(TextRange.FromBounds(start, start));
        }

        public IReadOnlyList<int> GetItemsContainingInclusiveEnd(int position)
        {
            return _collection.GetItemsContainingInclusiveEnd(position);
        }

        public int Count { get { return _collection.Count; } }

        /// <summary>
        /// Retrieves Nth item in the collection
        /// </summary>
        public T this[int index] { get { return _collection[index]; } }

                /// <summary>
        /// Returns index of item that starts at the given position if exists, -1 otherwise.
        /// </summary>
        /// <param name="position">Position in a text buffer</param>
        /// <returns>Item index or -1 if not found</returns>
        public int GetItemAtPosition(int position)
        {
            return _collection.GetItemAtPosition(position);
        }

        public int GetItemContaining(int position)
        {
            return _collection.GetItemContaining(position);
        }

        public int GetFirstItemBeforePosition(int position)
        {
            return _collection.GetFirstItemBeforePosition(position);
        }

        public int GetFirstItemAfterOrAtPosition(int position)
        {
            return _collection.GetFirstItemAfterOrAtPosition(position);
        }

        public T[] ToArray()
        {
            return _collection.ToArray();
        }

        #region IEnumerable<T> Members
        public IEnumerator<T> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }
        #endregion
    }
}

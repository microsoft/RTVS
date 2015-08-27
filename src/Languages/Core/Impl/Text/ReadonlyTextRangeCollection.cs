using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Languages.Core.Text
{
    public sealed class ReadOnlyTextRangeCollection<T> : IReadOnlyTextRangeCollection<T>, IEnumerable<T>, IEnumerable where T: ITextRange
    {
        public static readonly IReadOnlyTextRangeCollection<T> EmptyCollection = TextRangeCollection<T>.EmptyCollection;

        TextRangeCollection<T> collection;

        public ReadOnlyTextRangeCollection(TextRangeCollection<T> collection)
        {
            this.collection = collection;
        }

        public int Start { get { return this.collection.Start; } }
        public int End { get { return this.collection.End; } }

        public int Length { get { return this.collection.Length; } }

        public bool Contains(int position) { return this.collection.Contains(position); }

        public void Shift(int offset)
        {
            this.collection.Shift(offset);
        }

        public void ShiftStartingFrom(int position, int offset)
        {
            this.collection.ShiftStartingFrom(position, offset);
        }

        public IReadOnlyList<T> ItemsInRange(ITextRange range)
        {
            return this.collection.ItemsInRange(range);
        }

        public IReadOnlyList<T> ItemsInRange(int start)
        {
            return this.collection.ItemsInRange(TextRange.FromBounds(start, start));
        }

        public IReadOnlyList<int> GetItemsContainingInclusiveEnd(int position)
        {
            return this.collection.GetItemsContainingInclusiveEnd(position);
        }

        public int Count { get { return this.collection.Count; } }

        /// <summary>
        /// Retrieves Nth item in the collection
        /// </summary>
        public T this[int index] { get { return this.collection[index]; } }

                /// <summary>
        /// Returns index of item that starts at the given position if exists, -1 otherwise.
        /// </summary>
        /// <param name="position">Position in a text buffer</param>
        /// <returns>Item index or -1 if not found</returns>
        public int GetItemAtPosition(int position)
        {
            return this.collection.GetItemAtPosition(position);
        }

        public int GetItemContaining(int position)
        {
            return this.collection.GetItemContaining(position);
        }

        public int GetFirstItemBeforePosition(int position)
        {
            return this.collection.GetFirstItemBeforePosition(position);
        }

        public int GetFirstItemAfterOrAtPosition(int position)
        {
            return this.collection.GetFirstItemAfterOrAtPosition(position);
        }

        public T[] ToArray()
        {
            return this.collection.ToArray();
        }

        #region IEnumerable<T> Members
        public IEnumerator<T> GetEnumerator()
        {
            return this.collection.GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.collection.GetEnumerator();
        }
        #endregion
    }
}

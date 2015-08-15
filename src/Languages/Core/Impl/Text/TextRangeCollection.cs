using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Languages.Core.Text
{
    /// <summary>
    /// A collection of text ranges or objects that implement <seealso cref="ITextRange"/>. 
    /// Ranges must not overlap. Can be sorted by range start positions. Can be searched 
    /// in order to locate range that contains given position or range that starts 
    /// at a given position. The search is a binary search. Collection implements 
    /// <seealso cref="ITextRangeCollection"/>
    /// </summary>
    /// <typeparam name="T">A class or an interface that derives from <seealso cref="ITextRange"/></typeparam>
    [DebuggerDisplay("Count={Count}")]
    public class TextRangeCollection<T> : IEnumerable<T>, ITextRangeCollection<T> where T : ITextRange
    {
        public static readonly TextRangeCollection<T> EmptyCollection = new TextRangeCollection<T>();
        public static readonly IReadOnlyList<T> EmptyList = new T[0];
        public static readonly IReadOnlyList<int> EmptyListInt = new int[0];

        private List<T> items = new List<T>();

        #region Construction
        public TextRangeCollection()
        {
        }

        public TextRangeCollection(IEnumerable<T> ranges)
        {
            Add(ranges);
            Sort();
        }
        #endregion

        #region ITextRange

        public int Start
        {
            get { return Count > 0 ? this[0].Start : 0; }
        }

        public int End
        {
            get { return Count > 0 ? this[Count - 1].End : 0; }
        }

        public int Length
        {
            get { return End - Start; }
        }

        public virtual bool Contains(int position)
        {
            if (this.Count == 0)
                return false;

            return TextRange.Contains(this, position);
        }

        public void Shift(int offset)
        {
            this.ShiftFromIndex(0, offset);
        }
        #endregion

        #region ITextRangeCollection

        /// <summary>
        /// Number of comments in the collection.
        /// </summary>
        public int Count { get { return this.items.Count; } }

        /// <summary>
        /// Sorted list of comment tokens in the document.
        /// </summary>
        public IList<T> Items
        {
            get { return this.items; }
            protected set { this.items = new List<T>(value); }
        }

        /// <summary>
        /// Retrieves Nth item in the collection
        /// </summary>
        public T this[int index] { get { return this.items[index]; } }

        /// <summary>
        /// Adds item to collection.
        /// </summary>
        /// <param name="item">Item to add</param>
        public virtual void Add(T item)
        {
            this.items.Add(item);
        }

        /// <summary>
        /// Add a range of items to the collection
        /// </summary>
        /// <param name="items">Items to add</param>
        public void Add(IEnumerable<T> items)
        {
            this.items.AddRange(items);
        }

        /// <summary>
        /// Returns index of item that starts at the given position if exists, -1 otherwise.
        /// </summary>
        /// <param name="position">Position in a text buffer</param>
        /// <returns>Item index or -1 if not found</returns>
        public virtual int GetItemAtPosition(int position)
        {
            if (Count == 0)
                return -1;

            if (position < this[0].Start)
                return -1;

            if (position >= this[Count - 1].End)
                return -1;

            int min = 0;
            int max = Count - 1;

            while (min <= max)
            {
                int mid = min + (max - min) / 2;
                var item = this[mid];

                if (item.Start == position)
                    return mid;

                if (position < item.Start)
                {
                    max = mid - 1;
                }
                else
                {
                    min = mid + 1;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns index of items that contains given position if exists, -1 otherwise.
        /// </summary>
        /// <param name="position">Position in a text buffer</param>
        /// <returns>Item index or -1 if not found</returns>
        public virtual int GetItemContaining(int position)
        {
            if (Count == 0)
                return -1;

            if (position < this[0].Start)
                return -1;

            if (position > this[Count - 1].End)
                return -1;

            int min = 0;
            int max = Count - 1;

            while (min <= max)
            {
                int mid = min + (max - min) / 2;
                var item = this[mid];

                if (item.Contains(position))
                    return mid;

                if (mid < Count - 1 && item.End <= position && position < this[mid + 1].Start)
                    return -1;

                if (position < item.Start)
                {
                    max = mid - 1;
                }
                else
                {
                    min = mid + 1;
                }
            }

            return -1;
        }

        private bool IndexContainsPositionUsingInclusion(int index, int position)
        {
            bool containsPosition = false;
            ITextRange item = this[index];
            IExpandableTextRange expandableItem = item as IExpandableTextRange;

            if (expandableItem != null)
            {
                containsPosition = expandableItem.ContainsUsingInclusion(position);
            }
            else
            {
                containsPosition = item.Contains(position);
            }

            return containsPosition;
        }

        /// <summary>
        /// Returns index of items that contains given position if exists, -1 otherwise.
        /// </summary>
        /// <param name="position">Position in a text buffer</param>
        /// <returns>Item index if found, bitwise complement where a range with such a position would
        ///             be placed if not found</returns>
        public virtual int GetItemContainingUsingInclusion(int position, bool first)
        {
            if (Count == 0)
                return ~0;

            if ((position <= this[0].Start) && !IndexContainsPositionUsingInclusion(0, position))
                return ~0;

            if ((position >= this[Count - 1].End) && !IndexContainsPositionUsingInclusion(Count - 1, position))
                return ~Count;

            int min = 0;
            int max = Count - 1;

            while (min <= max)
            {
                int mid = min + (max - min) / 2;

                if (IndexContainsPositionUsingInclusion(mid, position))
                {
                    if (first)
                    {
                        while (mid > 0)
                        {
                            if (!IndexContainsPositionUsingInclusion(mid - 1, position))
                                return mid;
                            mid -= 1;
                        }
                        return 0;
                    }
                    else
                    {
                        while (mid + 1 < Count)
                        {
                            if (!IndexContainsPositionUsingInclusion(mid + 1, position))
                                return mid;
                            mid += 1;
                        }

                        return Count - 1;
                    }
                }

                if (min == max)
                    break;

                ITextRange item = this[mid];
                if (position <= item.Start)
                {
                    max = mid - 1;
                }
                else
                {
                    min = mid + 1;
                }
            }

            // Didn't find a match, either use min or (min + 1)
            ITextRange lastItem = this[min];
            if (position >= lastItem.End)
                min += 1;

            return ~min;
        }

        /// <summary>
        /// Retrieves first item that is after a given position
        /// </summary>
        /// <param name="position">Position in a text buffer</param>
        /// <returns>Item index or -1 if not found</returns>
        public virtual int GetFirstItemAfterOrAtPosition(int position)
        {
            if (Count == 0 || position > this[Count - 1].End)
                return -1;

            if (position < this[0].Start)
                return 0;

            int min = 0;
            int max = Count - 1;

            while (min <= max)
            {
                int mid = min + (max - min) / 2;
                var item = this[mid];

                if (item.Contains(position))
                {
                    // Note that there may be multiple items with the same range.
                    // To be sure we do pick the first one, walk back until we include
                    // all elements containing passed position
                    return GetFirstElementContainingPosition(mid, position);
                }

                if (mid > 0 && this[mid - 1].End <= position && item.Start >= position)
                {
                    return mid;
                }

                if (position < item.Start)
                {
                    max = mid - 1;
                }
                else
                {
                    min = mid + 1;
                }
            }

            return -1;
        }

        private int GetFirstElementContainingPosition(int index, int position)
        {
            for (int i = index - 1; i >= 0; i--)
            {
                var item = this[i];

                if (!item.Contains(position))
                {
                    index = i + 1;
                    break;
                }
                else if (i == 0)
                {
                    return 0;
                }
            }

            return index;
        }

        // assuming the item at index already contains the position
        private int GetLastElementContainingPosition(int index, int position)
        {
            for (int i = index; i < Count; i++)
            {
                var item = this[i];

                if (!item.Contains(position))
                {
                    return i - 1;
                }
            }

            return Count - 1;
        }

        /// <summary>
        /// Retrieves first item that is after a given position
        /// </summary>
        /// <param name="position">Position in a text buffer</param>
        /// <returns>Item index or -1 if not found</returns>
        public virtual int GetLastItemBeforeOrAtPosition(int position)
        {
            if (Count == 0 || position < this[0].Start)
                return -1;

            if (position >= this[Count - 1].End)
                return Count - 1;

            int min = 0;
            int max = Count - 1;

            while (min <= max)
            {
                int mid = min + (max - min) / 2;
                var item = this[mid];

                if (item.Contains(position))
                {
                    // Note that there may be multiple items with the same range.
                    // To be sure we do pick the first one, walk back until we include
                    // all elements containing passed position
                    return GetLastElementContainingPosition(mid, position);
                }

                // position is in between two tokens
                if (mid > 0 && this[mid - 1].End <= position && item.Start >= position)
                {
                    return mid - 1;
                }

                if (position < item.Start)
                {
                    max = mid - 1;
                }
                else
                {
                    min = mid + 1;
                }
            }

            return -1;
        }
        /// <summary>
        /// Retrieves first item that is before a given position, not intersecting or touching with position
        /// </summary>
        /// <param name="position">Position in a text buffer</param>
        /// <returns>Item index or -1 if not found</returns>
        public virtual int GetFirstItemBeforePosition(int position)
        {
            if (Count == 0 || position < this[0].End)
                return -1;

            int min = 0;
            int lastIndex = Count - 1;
            int max = Count - 1;

            if (position >= this[lastIndex].End)
                return max;

            while (min <= max)
            {
                int mid = min + (max - min) / 2;
                var item = this[mid];

                if (item.Contains(position)) // guaranteed not to be negative by the first if in this method
                {
                    return mid - 1;
                }

                if (mid < lastIndex && this[mid + 1].Start >= position && item.End <= position)
                {
                    return mid;
                }

                if (position < item.Start)
                {
                    max = mid - 1;
                }
                else
                {
                    min = mid + 1;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns index of items that contains given position if exists, -1 otherwise.
        /// </summary>
        /// <param name="position">Position in a text buffer</param>
        /// <returns>Item index or -1 if not found</returns>
        public virtual IReadOnlyList<int> GetItemsContainingInclusiveEnd(int position)
        {
            IReadOnlyList<int> list = TextRangeCollection<T>.EmptyListInt;

            if (Count > 0 &&
                position >= this[0].Start &&
                position <= this[Count - 1].End)
            {
                int min = 0;
                int max = Count - 1;

                while (min <= max)
                {
                    int mid = min + (max - min) / 2;
                    var item = this[mid];

                    if (item.Contains(position) || item.End == position)
                    {
                        list = GetItemsContainingInclusiveEndLinearFromAPoint(mid, position);
                        break;
                    }

                    if (mid < Count - 1 && item.End <= position && position < this[mid + 1].Start)
                        break;

                    if (position < item.Start)
                    {
                        max = mid - 1;
                    }
                    else
                    {
                        min = mid + 1;
                    }
                }
            }

            return list;
        }

        private IReadOnlyList<int> GetItemsContainingInclusiveEndLinearFromAPoint(int startingPoint, int position)
        {
            Debug.Assert(Count > 0 && startingPoint < Count, "Starting point not in token list");

            List<int> list = new List<int>();

            for (int i = startingPoint; i >= 0; i--)
            {
                var item = this[i];

                if (item.Contains(position) || item.End == position)
                {
                    list.Insert(0, i);
                }
                else
                {
                    break;
                }
            }

            if (startingPoint + 1 < Count)
            {
                for (int i = startingPoint + 1; i < Count; i++)
                {
                    var item = this[i];

                    if (item.Contains(position) || item.End == position)
                    {
                        list.Add(i);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return list;
        }

        #region ICompositeTextRange
        /// <summary>
        /// Shifts all tokens at of below given position by the specified offset.
        /// </summary>
        public virtual void ShiftStartingFrom(int position, int offset)
        {
            int min = 0;
            int max = Count - 1;

            if (Count == 0)
                return;

            if (position <= this[0].Start)
            {
                // all children are below the shifting point
                this.Shift(offset);
            }
            else
            {
                while (min <= max)
                {
                    int mid = min + (max - min) / 2;
                    ITextRange curRange = this[mid];

                    if ((curRange.Start <= position) && (position <= curRange.End))
                    {
                        // TODO: This doesn't support zero length ranges
                        // Found: item contains start position
                        var composite = curRange as ICompositeTextRange;
                        if (composite != null)
                        {
                            composite.ShiftStartingFrom(position, offset);
                        }
                        else
                        {
                            var expandable = curRange as IExpandableTextRange;
                            if (expandable != null)
                                expandable.Expand(0, offset);
                        }

                        // Now shift all remaining siblings that are below this one
                        this.ShiftFromIndex(mid + 1, offset);

                        return;
                    }
                    else if (mid < Count - 1 && curRange.End <= position && position <= this[mid + 1].Start)
                    {
                        // Between this item and the next sibling. Shift siblings
                        this.ShiftFromIndex(mid + 1, offset);

                        return;
                    }

                    // Position does not belong to this item and is not between item end and next item start
                    if (position < curRange.Start)
                    {
                        // Item is after the given position. There may be better items 
                        // before this one so limit search to the range ending in this item.
                        max = mid - 1;
                    }
                    else
                    {
                        // Proceed forward
                        min = mid + 1;
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Finds out items that overlap a text range
        /// </summary>
        /// <param name="range">Text range</param>
        /// <returns>List of items that overlap the range</returns>
        public virtual IReadOnlyList<T> ItemsInRange(ITextRange range)
        {
            List<T> list = null;

            int first = this.GetItemContaining(range.Start);
            if (first < 0)
            {
                first = this.GetFirstItemAfterOrAtPosition(range.Start);
            }

            if (first >= 0)
            {
                for (int i = first; i < Count; i++)
                {
                    if (this.items[i].Start >= range.End)
                        break;

                    if (TextRange.Intersect(this.items[i], range))
                    {
                        if (list == null)
                            list = new List<T>();

                        list.Add(this.items[i]);
                    }
                }
            }

            return (list != null ? list : TextRangeCollection<T>.EmptyList);
        }

        /// <summary>
        /// Removes items that overlap given text range
        /// </summary>
        /// <param name="range">Range to remove items in</param>
        /// <returns>Collection of removed items</returns>
        public IReadOnlyCollection<T> RemoveInRange(ITextRange range)
        {
            return this.RemoveInRange(range, false);
        }

        /// <summary>
        /// Removes items that overlap given text range
        /// </summary>
        /// <param name="range">Range to remove items in</param>
        /// <param name="inclusiveEnds">True if range end is inclusive</param>
        /// <returns>Collection of removed items</returns>
        public virtual IReadOnlyCollection<T> RemoveInRange(ITextRange range, bool inclusiveEnds)
        {
            int first = this.GetFirstItemAfterOrAtPosition(range.Start);
            if (first < 0 || (!inclusiveEnds && this[first].Start >= range.End) || (inclusiveEnds && this[first].Start > range.End))
            {
                return TextRangeCollection<T>.EmptyList;
            }

            List<T> removed = null;
            int lastCandidate = this.GetLastItemBeforeOrAtPosition(range.End);
            int last = -1;

            if (lastCandidate < first)
            {
                lastCandidate = first;
            }

            if (!inclusiveEnds && first >= 0)
            {
                for (int i = lastCandidate; i >= first; i--)
                {
                    var item = this.items[i];

                    if (item.Start < range.End)
                    {
                        last = i;
                        break;
                    }
                }
            }
            else
            {
                last = lastCandidate;
            }

            if (first >= 0 && last >= 0)
            {
                if (removed == null)
                    removed = new List<T>();

                for (int i = first; i <= last; i++)
                {
                    removed.Add(this.items[i]);
                }

                this.items.RemoveRange(first, last - first + 1);
            }

            return removed ?? TextRangeCollection<T>.EmptyList;
        }

        /// <summary>
        /// Reflects changes in text to the collection. Items are expanded and/or
        /// shifted according to the change. If change affects more than one
        /// range then affected items are removed.
        /// </summary>
        /// <param name="start">Starting position of the change.</param>
        /// <param name="oldLength">Length of the changed fragment before the change.</param>
        /// <param name="newLength">Length of text fragment after the change.</param>
        /// <returns>Collection or removed blocks</returns>
        public virtual IReadOnlyCollection<T> ReflectTextChange(int start, int oldLength, int newLength)
        {
            IReadOnlyCollection<T> removed = TextRangeCollection<T>.EmptyList;
            if (this.Count == 0)
            {
                return TextRangeCollection<T>.EmptyList;
            }

            int oldEnd = start + oldLength;
            int startIndex = this.GetItemContainingUsingInclusion(start, first: false);
            int endIndex = -1;

            if (startIndex >= 0)
            {
                // we found a range that contains the start
                if ((oldLength == 0) || this.IndexContainsPositionUsingInclusion(startIndex, oldEnd))
                {
                    // The change is contained within the start range
                    endIndex = startIndex;
                }
            }

            if (endIndex == -1)
            {
                if (oldLength == 0)
                {
                    // Zero length changes start and end in the same range
                    endIndex = startIndex;
                }
                else
                {
                    // Determine which range contains oldEnd
                    endIndex = this.GetItemContainingUsingInclusion(oldEnd, first: true);
                }
            }

            if (startIndex == endIndex)
            {
                this.ReflectTextChangeInBlock(startIndex, start, oldLength, newLength);
            }
            else
            {
                removed = this.ReflectTextChangeInBlocks(startIndex, endIndex, start, oldLength, newLength);
            }

            return removed;
        }

        private IReadOnlyCollection<T> ReflectTextChangeInBlocks(int startIndex, int endIndex, int start, int oldLength, int newLength)
        {
            IReadOnlyCollection<T> removed = TextRangeCollection<T>.EmptyList;
            bool startRangeFound = true;
            if (startIndex < 0)
            {
                startRangeFound = false;
                startIndex = ~startIndex; // points to first range after
            }

            bool endRangeFound = true;
            if (endIndex < 0)
            {
                endRangeFound = false;
                endIndex = ~endIndex; // points to first range after
            }

            int offset = newLength - oldLength;
            int oldEnd = start + oldLength;
            bool newTextConsumed = false;

            // Remove ranges
            int removalStartIndex = startIndex;
            if (startRangeFound)
            {
                bool startRangeRemoved = true;
                if (start > this.items[startIndex].Start)
                {
                    startRangeRemoved = false;
                }
                else if ((this.items[startIndex] is IExpandableTextRange) && (this.items[startIndex] as IExpandableTextRange).AllowZeroLength)
                {
                    startRangeRemoved = false;
                }

                if (!startRangeRemoved)
                {
                    removalStartIndex += 1;

                    // start lies inside a range which wasn't removed, update it
                    ICompositeTextRange composite = this.items[startIndex] as ICompositeTextRange;
                    if (composite != null)
                    {
                        composite.ShiftStartingFrom(start, offset);
                    }
                    else
                    {
                        IExpandableTextRange expandable = this.items[startIndex] as IExpandableTextRange;
                        if (expandable != null)
                        {
                            int startRangeEndOffset;
                            if (expandable.IsEndInclusive)
                            {
                                // The start range will grow to contain any new text
                                startRangeEndOffset = newLength + (start - expandable.End);
                                newTextConsumed = true;
                            }
                            else
                            {
                                // The start range is unable to grow to contain the new text
                                startRangeEndOffset = (start - expandable.End);
                            }

                            expandable.Expand(0, startRangeEndOffset);
                        }
                    }
                }
            }

            int removalEndIndex = endIndex;
            if (endRangeFound)
            {
                bool endRangeRemoved = true;
                if (oldEnd < this.items[endIndex].End)
                {
                    endRangeRemoved = false;
                }
                else if ((this.items[endIndex] is IExpandableTextRange) && (this.items[endIndex] as IExpandableTextRange).AllowZeroLength)
                {
                    endRangeRemoved = false;
                }

                if (!endRangeRemoved)
                {
                    // oldEnd lies inside a range which wasn't removed, update it
                    ICompositeTextRange composite = this.items[endIndex] as ICompositeTextRange;
                    if (composite != null)
                    {
                        composite.ShiftStartingFrom(start, offset);
                    }
                    else
                    {
                        IExpandableTextRange expandable = this.items[endIndex] as IExpandableTextRange;

                        if (expandable != null)
                        {
                            int newStart = start + newLength;
                            if (expandable.IsStartInclusive && !newTextConsumed)
                            {
                                newStart -= newLength;
                            }

                            expandable.Expand(newStart - expandable.Start, offset);
                        }
                    }

                    // The range at endIndex has now been updated, move it to point to the next range
                    endIndex += 1;
                }
                else
                {
                    removalEndIndex += 1;

                    // Advance endIndex as removalEndIndex was advanced, and we use it for
                    //   updating endIndex below
                    endIndex += 1;
                }
            }

            // delete from [removalStartIndex, removalEndIndex)
            int removalCount = removalEndIndex - removalStartIndex;
            if (removalCount > 0)
            {
                removed = this.items.GetRange(removalStartIndex, removalCount);
                this.items.RemoveRange(removalStartIndex, removalCount);

                endIndex -= removalCount;
            }

            // Now shift all remaining siblings that are below this one
            ShiftFromIndex(endIndex, offset);

            return removed;
        }

        private void ReflectTextChangeInBlock(int startIndex, int start, int oldLength, int newLength)
        {
            int offset = newLength - oldLength;
            if (startIndex >= 0)
            {
                ICompositeTextRange composite = this.items[startIndex] as ICompositeTextRange;
                if (composite != null)
                {
                    composite.ShiftStartingFrom(start, offset);
                }
                else
                {
                    IExpandableTextRange expandable = this.items[startIndex] as IExpandableTextRange;
                    if (expandable != null)
                    {
                        expandable.Expand(0, offset);
                    }
                }

                startIndex += 1;
            }
            else
            {
                startIndex = ~startIndex; // points to first range after
            }

            this.ShiftFromIndex(startIndex, offset);
        }

        private void ShiftFromIndex(int index, int offset)
        {
            for (int i = index; i < Count; i++)
            {
                this[i].Shift(offset);
            }
        }

        public bool IsEqual(IEnumerable<T> other)
        {
            int otherCount = 0;

            foreach (var item in other)
                otherCount++;

            if (this.Count != otherCount)
                return false;

            int i = 0;
            foreach (var item in other)
            {
                if (this[i].Start != item.Start)
                    return false;

                if (this[i].Length != item.Length)
                    return false;

                i++;
            }

            return true;
        }

        public virtual void RemoveAt(int index)
        {
            this.Items.RemoveAt(index);
        }

        public virtual void RemoveRange(int startIndex, int count)
        {
            this.items.RemoveRange(startIndex, count);
        }

        public virtual void Clear()
        {
            this.items.Clear();
        }

        public virtual void ReplaceAt(int index, T newItem)
        {
            this.items[index] = newItem;
        }

        public virtual void ReplaceRange(int index, int count, TextRangeCollection<T> collection)
        {
            index = Math.Max(index, 0);
            count = Math.Min(Count - index, count);

            int replaceCount = Math.Min(count, collection.Count);
            for (int i = 0; i < replaceCount; i++)
            {
                this.ReplaceAt(index + i, collection[i]);
            }

            if (replaceCount < collection.Count)
            {
                List<T> remainder = new List<T>(collection.Count - replaceCount);
                for (int i = replaceCount; i < collection.Count; i++)
                {
                    remainder.Add(collection[i]);
                }

                this.items.InsertRange(index + replaceCount, remainder);
            }
            else if (replaceCount < count)
            {
                this.RemoveRange(index + replaceCount, count - replaceCount);
            }
        }

        /// <summary>
        /// Sorts comment collection by token start positions.
        /// </summary>
        public void Sort()
        {
            this.items.Sort(new RangeItemComparer());
        }
        #endregion

        /// <summary>
        /// Returns collection of items in an array
        /// </summary>
        public T[] ToArray()
        {
            return this.items.ToArray();
        }

        /// <summary>
        /// Returns collection of items in a list
        /// </summary>
        public IList<T> ToList()
        {
            return new List<T>(this.items);
        }

        /// <summary>
        /// Compares two collections and calculates 'changed' range. In case this collection
        /// or comparand are empty, uses lowerBound and upperBound values as range
        /// delimiters. Typically lowerBound is 0 and upperBound is lentgh of the file.
        /// </summary>
        /// <param name="otherCollection">Collection to compare to</param>
        public virtual ITextRange RangeDifference(IEnumerable<ITextRange> otherCollection, int lowerBound, int upperBound)
        {
            if (otherCollection == null)
                return TextRange.FromBounds(lowerBound, upperBound);

            var other = new TextRangeCollection<ITextRange>(otherCollection);

            if (this.Count == 0 && other.Count == 0)
                return TextRange.EmptyRange;

            if (this.Count == 0)
                return TextRange.FromBounds(lowerBound, upperBound);

            if (other.Count == 0)
                return TextRange.FromBounds(lowerBound, upperBound);

            int minCount = Math.Min(this.Count, other.Count);
            int start = 0;
            int end = 0;
            int i, j;

            for (i = 0; i < minCount; i++)
            {
                start = Math.Min(this[i].Start, other[i].Start);

                if (this[i].Start != other[i].Start || this[i].Length != other[i].Length)
                    break;
            }

            if (i == minCount)
            {
                if (this.Count == other.Count)
                    return TextRange.EmptyRange;

                if (this.Count > other.Count)
                    return TextRange.FromBounds(Math.Min(upperBound, other[minCount - 1].Start), upperBound);
                else
                    return TextRange.FromBounds(Math.Min(this[minCount - 1].Start, upperBound), upperBound);
            }

            for (i = this.Count - 1, j = other.Count - 1; i >= 0 && j >= 0; i--, j--)
            {
                end = Math.Max(this[i].End, other[j].End);

                if (this[i].Start != other[j].Start || this[i].Length != other[j].Length)
                    break;
            }

            if (start < end)
                return TextRange.FromBounds(start, end);

            return TextRange.FromBounds(lowerBound, upperBound);
        }

        /// <summary>
        /// Merges another collection into existing one. Only adds elements
        /// that are not present in this collection. Both collections must
        /// be sorted by position for the method to work properly.
        /// </summary>
        /// <param name="other"></param>
        public void Merge(TextRangeCollection<T> other)
        {
            int i = 0;
            int j = 0;
            int count = this.Count;

            while (true)
            {
                if (i > count - 1)
                {
                    // Add elements remaining in the other collection
                    for (; j < other.Count; j++)
                    {
                        this.Add(other[j]);
                    }

                    break;
                }

                if (j > other.Count - 1)
                {
                    break;
                }

                if (this[i].Start < other[j].Start)
                {
                    i++;
                }
                else if (other[j].Start < this[i].Start)
                {
                    this.Add(other[j++]);
                }
                else
                {
                    // Element is already in the collection
                    j++;
                }
            }

            this.Sort();
        }

        class RangeItemComparer : IComparer<T>
        {
            #region IComparer<T> Members
            public int Compare(T x, T y)
            {
                return x.Start - y.Start;
            }
            #endregion
        }

        #region IEnumerable<T> Members
        public IEnumerator<T> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.items.GetEnumerator();
        }
        #endregion
    }
}

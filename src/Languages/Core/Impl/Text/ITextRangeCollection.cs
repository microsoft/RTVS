// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


using System.Collections.Generic;

namespace Microsoft.Languages.Core.Text {
    /// <summary>
    /// Represents collection of ITextRange items
    /// </summary>
    public interface ITextRangeCollection<T> : IReadOnlyTextRangeCollection<T> where T : ITextRange {
        /// <summary>
        /// Removes all items from collection
        /// </summary>
        void Clear();

        /// <summary>
        /// Appends text range to the collection. 
        /// Collection is not automatically sorted.
        /// </summary>
        void Add(T item);

        /// <summary>
        /// Appends collection of text ranges to the collection. 
        /// Collection is not automatically sorted.
        /// </summary>
        void Add(IEnumerable<T> items);

        /// <summary>
        /// Inserts text range into the collection in sorted order. 
        /// The collection must be sorted or the result is undefined.
        /// </summary>
        void AddSorted(T item);

        /// <summary>
        /// Removes item at a given index
        /// </summary>
        void RemoveAt(int index);

        /// <summary>
        /// Removes several items starting at index
        /// </summary>
        void RemoveRange(int startIndex, int count);

        /// <summary>
        /// Removes items that overlap given text range
        /// </summary>
        /// <param name="range">Range to remove items in</param>
        /// <returns>Collection of removed items</returns>
        ICollection<T> RemoveInRange(ITextRange range);

        /// <summary>
        /// Sorts ranges in collection by start position.
        /// </summary>
        void Sort();
    }
}

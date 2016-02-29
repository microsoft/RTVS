// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


using System.Collections.Generic;

namespace Microsoft.Languages.Core.Text {
    /// <summary>
    /// Represents collection of ITextRange items
    /// </summary>
    public interface IReadOnlyTextRangeCollection<T> : ICompositeTextRange, IReadOnlyList<T> {

        /// <summary>
        /// Returns index of item that starts at the given position if exists, -1 otherwise.
        /// </summary>
        /// <param name="position">Position in a text buffer</param>
        /// <returns>Item index or -1 if not found</returns>
        int GetItemAtPosition(int position);

        /// <summary>
        /// Returns index of items that contains given position if exists, -1 otherwise.
        /// </summary>
        /// <param name="position">Position in a text buffer</param>
        /// <returns>Item index or -1 if not found</returns>
        int GetItemContaining(int position);

        /// <summary>
        /// Retrieves item that immediately precedes given position
        /// </summary>
        int GetFirstItemBeforePosition(int position);

        /// <summary>
        /// Retrieves item that is at or immediately follows the position
        /// </summary>
        int GetFirstItemAfterOrAtPosition(int position);

        /// <summary>
        /// Finds items that overlap a text range
        /// </summary>
        /// <param name="range">Text range</param>
        /// <returns>List of items that overlap the range</returns>
        IReadOnlyList<T> ItemsInRange(ITextRange range);
    }
}

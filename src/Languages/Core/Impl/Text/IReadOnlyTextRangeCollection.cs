
using System.Collections.Generic;

namespace Microsoft.Languages.Core.Text
{
    /// <summary>
    /// Represents collection of ITextRange items
    /// </summary>
    public interface IReadOnlyTextRangeCollection<T> : ICompositeTextRange, IReadOnlyCollection<T>
    {
        /// <summary>
        /// Retrieves Nth item in the collection
        /// </summary>
        T this[int index] { get; }

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
    }
}

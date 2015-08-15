
using System.Collections.Generic;

namespace Microsoft.Languages.Core.Text
{
    /// <summary>
    /// Represents collection of ITextRange items
    /// </summary>
    public interface ITextRangeCollection<T> : IReadOnlyTextRangeCollection<T> where T : ITextRange
    {
        /// <summary>
        /// Removes all items from collection
        /// </summary>
        void Clear();

        /// <summary>
        /// Sorts ranges in collection by start position.
        /// </summary>
        void Sort();
    }
}

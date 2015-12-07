using System.Collections.Generic;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Represents a provider of collection details.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    public interface IItemsProvider<T> {
        /// <summary>
        /// The total number of items
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Return items
        /// </summary>
        /// <param name="startIndex">first item's index (zero based)</param>
        /// <param name="count">number of items to return</param>
        /// <returns>reqeusted items</returns>
        IList<T> Acquire(int startIndex, int count);

        /// <summary>
        /// Hint to the provider that user doesn't use any longer
        /// </summary>
        /// <param name="startIndex">first item's index (zero based)</param>
        /// <param name="count">number of items to return</param>
        void Release(int startIndex, int count);
    }
}

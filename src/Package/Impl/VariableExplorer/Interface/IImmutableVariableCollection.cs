using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.VariableWindow {
    /// <summary>
    /// Represents an immutable collection of variables.
    /// </summary>
    public interface IImmutableVariableCollection {
        /// <summary>
        /// The number of variables in this collection.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Returns the requested variable.
        /// </summary>
        /// <param name="index">The index of the requested variable</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="IndexOutOfRangeException">
        /// <paramref name="index"/> is not within the valid range.
        /// </exception>
        Task<IVariable> GetAsync(int index, CancellationToken cancellationToken);

        /// <summary>
        /// Returns a sequence of contiguous variables.
        /// </summary>
        /// <param name="firstIndex">The first index to return</param>
        /// <param name="count">The number of variables to return</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="IndexOutOfRangeException">
        /// <paramref name="firstIndex"/> is not within the valid range, or
        /// <paramref name="count"/> extends beyond the valid range.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="count"/> is less than or equal to zero.
        /// </exception>
        Task<ICollection<IVariable>> GetManyAsync(int firstIndex, int count, CancellationToken cancellationToken);
    }
}

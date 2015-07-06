using System.Collections.Generic;

namespace Microsoft.R.Core.AST.DataTypes.Definitions
{
    /// <summary>
    /// Represents R vector. Most if not all R objects are vectors of some sort.
    /// R vector is essentially a dynamically growing array. It allows appending
    /// items (even while leaving gaps) but unlike list it does not allow insertion 
    /// between existing items.
    /// </summary>
    public interface IRVector<T>: IRVector, IEnumerable<T>
    {
        /// <summary>
        /// Indexer
        /// </summary>
        T this[int index] { get; }
    }
}

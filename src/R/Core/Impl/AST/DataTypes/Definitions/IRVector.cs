using System.Collections.Generic;

namespace Microsoft.R.Core.AST.DataTypes.Definitions {
    /// <summary>
    /// Represents R vector. Most if not all R objects are vectors of some sort.
    /// R vector is essentially a dynamically growing array. It allows appending
    /// items (even while leaving gaps) but unlike list it does not allow insertion 
    /// between existing items.
    /// </summary>
    public interface IRVector {
        /// <summary>
        /// Vector mode (data type)
        /// </summary>
        RMode Mode { get; }

        /// <summary>
        /// Number of elements in the vector
        /// </summary>
        int Length { get; }

        /// <summary>
        /// Names assigned to the vector elements
        /// </summary>
        //IReadOnlyCollection<string> Names { get; set; }
    }
}

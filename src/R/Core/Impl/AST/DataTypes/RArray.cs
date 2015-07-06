using Microsoft.R.Core.AST.DataTypes.Definitions;

namespace Microsoft.R.Core.AST.DataTypes
{
    /// <summary>
    /// Implements R array. Array is a vector with optional
    /// name of the dimension. Dimension names are typically
    /// used in multi-dimensional arrays which are effecively
    /// arrays of array.
    /// </summary>
    public class RArray<T>: RVector<T>, IRArray<T>
    {
        /// <summary>
        /// Dimension name. Mostly used in multi-dimensional cases.
        /// </summary>
        public RString DimName { get; set; }

        public RArray(RMode mode, int length):
            base(mode, length)
        {
        }
    }
}

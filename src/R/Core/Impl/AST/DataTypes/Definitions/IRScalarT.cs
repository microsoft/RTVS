using System.Collections.Generic;

namespace Microsoft.R.Core.AST.DataTypes.Definitions
{
    /// <summary>
    /// Represents scalar (numerical, string, boolean) value. 
    /// Scalars are one-element vectors.
    /// </summary>
    public interface IRScalar<T>
    {
        T Value { get; set; }
    }
}

namespace Microsoft.R.Core.AST.DataTypes.Definitions
{
    public interface IRArray<T>: IRVector<T>
    {
        /// <summary>
        /// Dimension name. Mostly used in multi-dimensional cases.
        /// </summary>
        RString DimName { get; set; }
    }
}

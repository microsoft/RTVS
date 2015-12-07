namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Abstract two dimentional data
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGrid<T> {
        /// <summary>
        /// number of rows
        /// </summary>
        int RowCount { get; }

        /// <summary>
        /// number of columns
        /// </summary>
        int ColumnCount { get; }

        /// <summary>
        /// Return value at given position
        /// </summary>
        /// <param name="rowIndex">row index, zero based</param>
        /// <param name="columnIndex">column index, zero based</param>
        /// <returns>item value</returns>
        /// <exception cref="ArgumentOutOfRangeException">when index is out of range</exception>
        /// <exception cref="InvalidOperationException">when failed at setting or getting the value</exception>
        /// <exception cref="NotSupportedException">setter, when the grid is read only</exception>
        T this[int rowIndex, int columnIndex] {
            get; set;
        }
    }
}

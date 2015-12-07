namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// tow dimensional range
    /// </summary>
    public struct GridRange {
        public GridRange(Range rows, Range columns) {
            Rows = rows;
            Columns = columns;
        }

        public Range Rows { get; }

        public Range Columns { get; }

        public bool Contains(int row, int column) {
            return Rows.Contains(row) && Columns.Contains(column);
        }
    }
}

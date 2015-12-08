namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal struct PageNumber {
        public PageNumber(int row, int column) {
            Row = row;
            Column = column;
        }

        public int Row { get; }

        public int Column { get; }
    }
}

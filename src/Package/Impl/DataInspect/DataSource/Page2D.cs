using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public class Page2D<T> {
        private Grid<PageItem<T>> _grid;

        public Page2D(PageNumber pageNumber, GridRange range) {
            PageNumber = pageNumber;
            Range = range;
            Node = new LinkedListNode<Page2D<T>>(this);
            LastAccessTime = DateTime.MinValue;

            _grid = new Grid<PageItem<T>>(
                range.Rows.Count,
                range.Columns.Count,
                (r, c) => new PageItem<T>(range.Columns.Start + c));
        }

        public PageNumber PageNumber { get; }

        public GridRange Range { get; }

        public LinkedListNode<Page2D<T>> Node;

        public DateTime LastAccessTime { get; set; }

        public PageItem<T> GetItem(int row, int column) {
            Debug.Assert(Range.Contains(row, column));

            return _grid[row - Range.Rows.Start, column - Range.Columns.Start];
        }

        internal void PopulateData(IGrid<T> data) {
            if (data.RowCount != Range.Rows.Count || data.ColumnCount != Range.Columns.Count) {
                throw new ArgumentException("Input data doesn't match with page's row or column counts");
            }

            for (int r = 0; r < data.RowCount; r++) {
                for (int c = 0; c < data.ColumnCount; c++) {
                    _grid[r, c].Data = data[r, c];
                }
            }
        }
    }
}

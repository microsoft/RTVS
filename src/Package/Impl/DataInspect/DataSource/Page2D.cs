using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class Page2D<T> {
        private Grid<PageItem<T>> _grid;

        public Page2D(PageNumber pageNumber, GridRange range) {
            PageNumber = pageNumber;
            Range = range;
            Node = new LinkedListNode<Page2D<T>>(this);
            LastAccessTime = DateTime.UtcNow;

            _grid = new Grid<PageItem<T>>(
                range,
                (r, c) => new PageItem<T>(c));
        }

        public PageNumber PageNumber { get; }

        public GridRange Range { get; }

        public LinkedListNode<Page2D<T>> Node;

        public DateTime LastAccessTime { get; set; }

        public PageItem<T> GetItem(int row, int column) {
            return _grid[row, column];
        }

        internal void PopulateData(IGrid<T> data) {
            if (!data.Range.Equals(Range)) {
                throw new ArgumentException("Input data doesn't match with page's row or column counts");
            }

            foreach (int r in data.Range.Rows.GetEnumerable()) {
                foreach (int c in data.Range.Columns.GetEnumerable()) {
                    _grid[r, c].Data = data[r, c];
                }
            }
        }
    }
}

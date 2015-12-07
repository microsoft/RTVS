using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public class Grid<T> : IGrid<T> {

        private IList<T> _list;

        public Grid(int rowCount, int columnCount, Func<int, int, T> CreateNew) {
            RowCount = rowCount;
            ColumnCount = columnCount;

            var list = new List<T>(rowCount * columnCount);
            for (int c = 0; c < columnCount; c++) {
                for (int r = 0; r < rowCount; r++) {
                    list.Add(CreateNew(r, c));
                }
            }

            _list = list;
        }

        public Grid(int rowCount, int columnCount, IList<T> list) {
            RowCount = rowCount;
            ColumnCount = columnCount;

            if (list.Count < (RowCount * ColumnCount)) {
                throw new ArgumentException("list doesn't contain enough data");
            }

            _list = list;
        }

        public T this[int rowIndex, int columnIndex]
        {
            get
            {
                return _list[(columnIndex * RowCount) + rowIndex];
            }

            set
            {
                _list[(columnIndex * RowCount) + rowIndex] = value;
            }
        }

        public int ColumnCount { get; }

        public int RowCount { get; }
    }
}

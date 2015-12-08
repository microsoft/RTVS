using System;
using System.Collections.Generic;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Simple <see cref="IGrid{T}"/> implementation with linearized grid <see cref="List{T}"/>
    /// </summary>
    /// <typeparam name="T">type of item value</typeparam>
    public class Grid<T> : IGrid<T> {
        private IList<T> _list;

        /// <summary>
        /// Constructs and fill grid with generated items
        /// </summary>
        /// <param name="CreateNew">function to generate item</param>
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

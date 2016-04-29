// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Simple <see cref="IGrid{T}"/> implementation with linearized grid <see cref="List{T}"/>
    /// </summary>
    /// <typeparam name="T">type of item value</typeparam>
    public class Grid<T> : IGrid<T> {
        private IList<T> _list;

        public Grid(GridRange range, Func<int, int, T> createNew) {
            Range = range;

            _list = new List<T>(range.Rows.Count * range.Columns.Count);

            foreach (int c in range.Columns.GetEnumerable()) {
                foreach (int r in range.Rows.GetEnumerable()) {
                    _list.Add(createNew(r, c));
                }
            }
        }

        public Grid(GridRange range, IList<T> list) {
            if (list.Count != range.Rows.Count * range.Columns.Count) {
                throw new ArgumentException("Length of the initialization vector doesn't match grid dimensions");
            }

            Range = range;

            _list = list;
        }

        public T this[int rowIndex, int columnIndex]
        {
            get
            {
                return _list[ListIndex(rowIndex, columnIndex)];
            }

            set
            {
                _list[ListIndex(rowIndex, columnIndex)] = value;
            }
        }

        public GridRange Range { get; }

        private int ListIndex(int rowIndex, int columnIndex) {
            return ((columnIndex - Range.Columns.Start) * Range.Rows.Count) + (rowIndex - Range.Rows.Start);
        }
    }

    /// <summary>
    /// an adapter from IRange to IGrid
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RangeToGrid<T> : IGrid<T> {
        IRange<T> _data;
        Func<int, int, T> _getItemFunc;
        Action<int, int, T> _setItemFunc;

        public RangeToGrid(Range range, IRange<T> data, bool takeColumn) {
            if (takeColumn) {
                Range = new GridRange(new Range(0, 1), range);
                _getItemFunc = GetItemColumnMode;
                _setItemFunc = SetItemColumnMode;
            } else {
                Range = new GridRange(range, new Range(0, 1));
                _getItemFunc = GetItemRowMode;
                _setItemFunc = SetItemRowMode;
            }

            _data = data;
        }

        public T this[int rowIndex, int columnIndex] {
            get {
                return _getItemFunc(rowIndex, columnIndex);
            }

            set {
                _setItemFunc(rowIndex, columnIndex, value);
            }
        }

        public GridRange Range { get; }

        private T GetItemColumnMode(int rowIndex, int columnIndex) {
            if (rowIndex != 0) {
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }

            return _data[columnIndex];
        }

        private void SetItemColumnMode(int rowIndex, int columnIndex, T value) {
            if (rowIndex != 0) {
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }

            _data[columnIndex] = value;
        }

        private T GetItemRowMode(int rowIndex, int columnIndex) {
            if (columnIndex != 0) {
                throw new ArgumentOutOfRangeException(nameof(columnIndex));
            }

            return _data[rowIndex];
        }

        private void SetItemRowMode(int rowIndex, int columnIndex, T value) {
            if (columnIndex != 0) {
                throw new ArgumentOutOfRangeException(nameof(columnIndex));
            }

            _data[rowIndex] = value;
        }
    }

    public class GridByList<T> : IGrid<T> {
        List<List<T>> _data;

        public GridByList(GridRange range, List<List<T>> data) {
            Range = range;

            _data = data;
        }
        public GridRange Range { get; }

        public T this[int rowIndex, int columnIndex] {
            get {
                return _data[columnIndex - Range.Columns.Start][rowIndex - Range.Rows.Start];
            }

            set {
                _data[columnIndex - Range.Columns.Start][rowIndex - Range.Rows.Start] = value;
            }
        }
    }
}

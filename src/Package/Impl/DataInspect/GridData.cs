using System;
using System.Collections.Generic;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class GridData : IGridData<string> {
        public GridData() {
            RowNames = new List<string>();
            ColumnNames = new List<string>();
            Values = new List<List<string>>();
        }

        public bool ValidHeaderNames { get; set; }

        public List<string> RowNames { get; }

        public List<string> ColumnNames { get; }

        public List<List<string>> Values { get; }

        // TODO: the instantiation of this class seems weird. Clean up
        public GridRange Range { get; set; }

        private IRange<string> _columnHeader;
        public IRange<string> ColumnHeader {
            get {
                if (_columnHeader == null) {
                    if (ValidHeaderNames) {
                        _columnHeader = new ListToRange<string>(
                            Range.Columns,
                            ColumnNames);
                    } else {
                        _columnHeader = new DefaultHeaderData(Range.Columns, DefaultHeaderData.Mode.Column);
                    }
                }
                return _columnHeader;
            }
        }

        private IRange<string> _rowHeader;
        public IRange<string> RowHeader {
            get {
                if (_rowHeader == null) {
                    if (ValidHeaderNames) {
                        _rowHeader = new ListToRange<string>(
                            Range.Rows,
                            RowNames);
                    } else {
                        _rowHeader = new DefaultHeaderData(Range.Rows, DefaultHeaderData.Mode.Row);
                    }
                }

                return _rowHeader;
            }
        }

        private IGrid<string> _grid;
        public IGrid<string> Grid {
            get {
                if (_grid == null) {
                    _grid = new Grid<string>(
                        Range,
                        (r, c) => Values[c - Range.Columns.Start][r - Range.Rows.Start]);
                }

                return _grid;
            }
        }
    }

    internal class ListToRange<T> : IRange<T> {
        private IList<T> _list;

        public ListToRange(Range range, IList<T> list) {
            if (range.Count != list.Count) {
                throw new ArgumentException("Range data cound doesn't match with range");
            }

            Range = range;

            _list = list;
        }

        public Range Range { get; }

        public T this[int index] {
            get {
                return _list[index - Range.Start];
            }

            set {
                _list[index - Range.Start] = value;
            }
        }
    }
}

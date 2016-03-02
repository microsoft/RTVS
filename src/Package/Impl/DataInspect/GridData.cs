// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal class GridData : IGridData<string> {
        [Flags]
        public enum HeaderNames {
            None = 0,
            Row = 1,
            Column = 2,
        }


        public GridData(
            IList<string> rowNames,
            IList<string> columnNames,
            IList<string> values) {
            RowNames = rowNames;
            ColumnNames = columnNames;
            Values = values;
        }

        public HeaderNames ValidHeaderNames { get; set; }

        public IList<string> RowNames { get; }

        public IList<string> ColumnNames { get; }

        public IList<string> Values { get; }

        public GridRange Range { get; set; }

        private IRange<string> _columnHeader;
        public IRange<string> ColumnHeader {
            get {
                if (_columnHeader == null) {
                    if (ValidHeaderNames.HasFlag(HeaderNames.Column)) {
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
                    if (ValidHeaderNames.HasFlag(HeaderNames.Row)) {
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
                        Values);
                }

                return _grid;
            }
        }
    }
}

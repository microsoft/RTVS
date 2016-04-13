// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    [DataContract]
    internal class GridData : IGridData<string> {
        [DataMember(Name = "row.start")]
        int RowStart { get; set; }

        [DataMember(Name = "row.count")]
        int RowCount { get; set; }

        [DataMember(Name = "row.names")]
        public ReadOnlyCollection<string> RowNames { get; set; }

        [DataMember(Name = "col.start")]
        int ColumnStart { get; set; }

        [DataMember(Name = "col.count")]
        int ColumnCount { get; set; }

        [DataMember(Name = "col.names")]
        public ReadOnlyCollection<string> ColumnNames { get; set; }

        [DataMember(Name = "data")]
        public ReadOnlyCollection<string> Values { get; set; }

        public GridRange Range { get; set; }

        private IRange<string> _columnHeader;
        public IRange<string> ColumnHeader {
            get {
                if (_columnHeader == null) {
                    if (ColumnNames != null && ColumnNames.Count > 0) {
                        _columnHeader = new ListToRange<string>(
                            new Range(ColumnStart - 1, ColumnCount),
                            ColumnNames);
                    } else {
                        _columnHeader = new DefaultHeaderData(new Range(ColumnStart - 1, ColumnCount), DefaultHeaderData.Mode.Column);
                    }
                }
                return _columnHeader;
            }
        }

        private IRange<string> _rowHeader;
        public IRange<string> RowHeader {
            get {
                if (_rowHeader == null) {
                    if (RowNames != null && RowNames.Count > 0) {
                        _rowHeader = new ListToRange<string>(
                            new Range(RowStart - 1, RowCount),
                            RowNames);
                    } else {
                        _rowHeader = new DefaultHeaderData(new Range(RowStart - 1, RowCount), DefaultHeaderData.Mode.Row);
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
                        new GridRange(new Range(RowStart - 1, RowCount), new Range(ColumnStart - 1, ColumnCount)),
                        Values);
                }

                return _grid;
            }
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.R.Host.Client;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    [DataContract]
    internal class GridData : IGridData<string> {
        [DataMember(Name = "is_1d")]
        bool Is1D { get; set; }

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

        private IRange<string> MakeHeader(DefaultHeaderData.Mode mode, IReadOnlyCollection<string> names, int start, int count, ref IRange<string> cache) {
            if (cache == null) {
                var defaultHeader = new DefaultHeaderData(new Range(start - 1, count), mode, Is1D);
                if (names != null && names.Count > 0) {
                    var namesOrDefault = names.Select((name, i) => name == null ? defaultHeader[i] : name.ConvertCharacterCodes());
                    cache = new ListToRange<string>(new Range(start - 1, count), namesOrDefault.ToList());
                } else {
                    cache = defaultHeader;
                }
            }

            return cache;
        }

        private IRange<string> _columnHeader;
        public IRange<string> ColumnHeader => MakeHeader(DefaultHeaderData.Mode.Column, ColumnNames, ColumnStart, ColumnCount, ref _columnHeader);


        private IRange<string> _rowHeader;
        public IRange<string> RowHeader => MakeHeader(DefaultHeaderData.Mode.Row, RowNames, RowStart, RowCount, ref _rowHeader);

        private IGrid<string> _grid;
        public IGrid<string> Grid {
            get {
                _grid = _grid ?? new Grid<string>(
                    new GridRange(new Range(RowStart - 1, RowCount), new Range(ColumnStart - 1, ColumnCount)),
                    Values.Select(s => s.ConvertCharacterCodes()).ToList());
                return _grid;
            }
        }
    }
}

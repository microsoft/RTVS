// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// tow dimensional range
    /// </summary>
    [DebuggerDisplay("[{Columns.Start},{Columns.End})X[{Rows.Start},{Rows.End})")]
    public struct GridRange {
        public Range Columns { get; }
        public Range Rows { get; }

        public GridRange(Range rows, Range columns) {
            Rows = rows;
            Columns = columns;
        }

        public bool Contains(long row, long column) => Rows.Contains(row) && Columns.Contains(column);
        public bool Contains(GridRange other) => Rows.Contains(other.Rows) && Columns.Contains(other.Columns);
    }
}

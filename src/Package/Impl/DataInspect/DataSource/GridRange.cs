// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// tow dimensional range
    /// </summary>
    public struct GridRange {
        public GridRange(Range rows, Range columns) {
            Rows = rows;
            Columns = columns;
        }

        public Range Rows { get; }

        public Range Columns { get; }

        public bool Contains(long row, long column) {
            return Rows.Contains(row) && Columns.Contains(column);
        }

        public bool Contains(GridRange other) {
            return Rows.Contains(other.Rows) && Columns.Contains(other.Columns);
        }
    }
}

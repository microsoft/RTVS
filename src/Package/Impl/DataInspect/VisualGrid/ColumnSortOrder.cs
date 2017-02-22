// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public class ColumnSortOrder {
        public long ColumnIndex { get; }
        public bool Descending { get; set; }
        public ColumnSortOrder(long columnIndex, bool descending) {
            ColumnIndex = columnIndex;
            Descending = descending;
        }
    }
}

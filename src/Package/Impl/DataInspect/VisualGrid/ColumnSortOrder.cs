// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    public class ColumnSortOrder {
        public string ColumnName { get; }
        public bool Descending { get; set; }
        public ColumnSortOrder(string columnName, bool descending) {
            ColumnName = columnName;
            Descending = descending;
        }
    }
}

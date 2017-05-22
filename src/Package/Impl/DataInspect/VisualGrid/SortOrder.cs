// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    /// <summary>
    /// Describes sorting set in details grid
    /// </summary>
    internal sealed class SortOrder : ISortOrder {
        private readonly List<ColumnSortOrder> _sortOrderList = new List<ColumnSortOrder>();

        /// <summary>
        /// No sorting is specified
        /// </summary>
        public bool IsEmpty => _sortOrderList.Count == 0;

        /// <summary>
        /// Resets sort to one column assuming default (ascending) order.
        /// Typically called as a result of click on one of the column
        /// headers which clears current sort across all columns and sets
        /// the clicked column as primary with the default sorting order.
        /// </summary>
        /// <param name="v"></param>
        public void ResetTo(HeaderTextVisual v) {
            _sortOrderList.Clear();
            _sortOrderList.Add(new ColumnSortOrder(v.ColumnIndex, v.SortOrder == SortOrderType.Descending));
        }

        /// <summary>
        /// Adds column to the sorting order. Typically called when user
        /// Shift+Click on the column header adding it to the set. 
        /// If the column is already in the set, the call does nothing.
        /// </summary>
        /// <param name="v"></param>
        public void Add(HeaderTextVisual v) {
            var existing = _sortOrderList.FirstOrDefault(x => x.ColumnIndex == v.ColumnIndex);
            if (existing == null) {
                _sortOrderList.Add(new ColumnSortOrder(v.ColumnIndex, v.SortOrder == SortOrderType.Descending));
            } else {
                existing.Descending = v.SortOrder == SortOrderType.Descending;
            }
        }

        public void Add(ColumnSortOrder order) {
            _sortOrderList.Add(order);
        }

        /// <remarks>
        /// Complete expression looks like:
        /// <code>
        /// function(x) order(x[,col_idx1], -x[,col_idx1], ...)
        /// </code>
        /// where minus tells R that the column sort order is descending rather than ascending.
        /// </remarks>
        public string GetRowSelector() {
            var sb = new StringBuilder("function(x) rtvs:::grid_order(x");

            foreach (var s in _sortOrderList) {
                sb.Append(", ");

                if (s.Descending) {
                    sb.Append('-');
                }

                sb.Append(s.ColumnIndex + 1);
            }

            sb.Append(")");
            return sb.ToString();
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Common.Core;
using static System.FormattableString;

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
        /// Returns sort direction of the primary column
        /// </summary>
        public bool IsPrimaryDescending {
            get { return !IsEmpty ? _sortOrderList[0].Descending : false; }
        }

        /// <summary>
        /// Resets sort to one column assuming default (ascending) order.
        /// Typically called as a result of click on one of the column
        /// headers which clears current sort across all columns and sets
        /// the clicked column as primary with the default sorting order.
        /// </summary>
        /// <param name="v"></param>
        public void ResetTo(HeaderTextVisual v) {
            _sortOrderList.Clear();
            _sortOrderList.Add(new ColumnSortOrder(v.Name, v.SortOrder == SortOrderType.Descending));
        }

        /// <summary>
        /// Adds column to the sorting order. Typically called when user
        /// Shift+Click on the column header adding it to the set. 
        /// If the column is already in the set, the call does nothing.
        /// </summary>
        /// <param name="v"></param>
        public void Add(HeaderTextVisual v) {
            var existing = _sortOrderList.FirstOrDefault(x => x.ColumnName.EqualsOrdinal(v.Name));
            if (existing == null) {
                _sortOrderList.Add(new ColumnSortOrder(v.Name, v.SortOrder == SortOrderType.Descending));
            } else {
                existing.Descending = v.SortOrder == SortOrderType.Descending;
            }
        }

        /// <summary>
        /// Constructs expression to evaluate in R when ordering the data frame.
        /// Complete expression looks like 
        /// 'do.call(order, c(x.df['col_name1'], -x.df['col_name2'], ...))'
        /// where x.df is name of the data frame in grid.r and minus tells R
        /// that the column sort order is descending rather than ascending.
        /// </summary>
        public string GetDataFrameSortExpression() {
            var sb = new StringBuilder("do.call(order, c(");
            bool first = true;
            foreach (var s in _sortOrderList) {
                if (!first) {
                    sb.Append(", ");
                } else {
                    first = false;
                }
                if (s.Descending) {
                    sb.Append('-');
                }
                // NOTE: name must match name of the data frame in grid.r
                sb.Append(Invariant($"x.df['{s.ColumnName}']"));
            }
            sb.Append("))");
            return sb.ToString();
        }
    }
}

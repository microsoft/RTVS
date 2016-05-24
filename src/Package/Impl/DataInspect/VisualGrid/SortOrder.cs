// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Common.Core;
using static System.FormattableString;

namespace Microsoft.VisualStudio.R.Package.DataInspect {
    internal sealed class SortOrder: ISortOrder {
        private readonly List<ColumnSortOrder> _sortOrderList = new List<ColumnSortOrder>();

        public bool IsEmpty => _sortOrderList.Count == 0;

        public void ResetTo(HeaderTextVisual v) {
            _sortOrderList.Clear();
            _sortOrderList.Add(new ColumnSortOrder(v.Name, v.SortOrder == SortOrderType.Descending));
        }

        public void Add(HeaderTextVisual v) {
            var existing = _sortOrderList.FirstOrDefault(x => x.ColumnName.EqualsOrdinal(v.Name));
            if (existing == null) {
                _sortOrderList.Add(new ColumnSortOrder(v.Name, v.SortOrder == SortOrderType.Descending));
            } else {
                existing.Descending = v.SortOrder == SortOrderType.Descending;
            }
        }

        public string GetSortExpression() {
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
                sb.Append(Invariant($"x.df['{s.ColumnName}']"));
            }
            sb.Append("))");
            return sb.ToString();
        }
    }
}

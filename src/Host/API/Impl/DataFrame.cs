// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Diagnostics;

namespace Microsoft.R.Host.Client {
    public sealed class DataFrame {
        public IReadOnlyList<string> ColumnNames { get; }
        public IReadOnlyList<string> RowNames { get; }
        public IReadOnlyList<IReadOnlyList<object>> Data { get; }

        public DataFrame(IReadOnlyCollection<string> rowNames, IReadOnlyCollection<string> columnNames, IReadOnlyCollection<IReadOnlyCollection<object>> data) {
            Check.ArgumentNull(nameof(data), data);
            Check.ArgumentOutOfRange(nameof(data), () => data.Count == 0);
            Check.ArgumentOutOfRange(nameof(rowNames), () => rowNames != null && rowNames.Count != data.First().Count);
            Check.ArgumentOutOfRange(nameof(columnNames), () => columnNames != null && columnNames.Count != data.Count);
            Check.ArgumentOutOfRange(nameof(data), () => data.Where(c => c.Count != rowNames.Count).Any());

            RowNames = new List<string>(rowNames);
            ColumnNames = new List<string>(columnNames);

            var list = new List<List<object>>();
            foreach(var e in data) {
                list.Add(new List<object>(e));
            }
            Data = list;
        }

        public IReadOnlyList<object> GetColumn(string name) {
            var selection = ColumnNames.IndexWhere(x => x.EqualsOrdinal(name));
            return selection.Any() ? Data[selection.First()] : null;
        }
    }
}

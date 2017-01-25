// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Diagnostics;

namespace Microsoft.R.Host.Client {
    /// <summary>
    /// Represents R data frame
    /// </summary>
    public sealed class DataFrame {
        /// <summary>
        /// Column names
        /// </summary>
        public IReadOnlyList<string> ColumnNames { get; }

        /// <summary>
        /// Row names
        /// </summary>
        public IReadOnlyList<string> RowNames { get; }

        /// <summary>
        /// Data frame data
        /// </summary>
        public IReadOnlyList<IReadOnlyList<object>> Data { get; }
        
        /// <summary>
        /// Constructs data frame
        /// </summary>
        /// <param name="rowNames">Row names</param>
        /// <param name="columnNames">Column names</param>
        /// <param name="data">Data</param>
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

        /// <summary>
        /// Retrieves data frame column by name
        /// </summary>
        /// <param name="name">Column name</param>
        /// <returns>Column data</returns>
        public IReadOnlyList<object> GetColumn(string name) {
            var selection = ColumnNames.IndexWhere(x => x.EqualsOrdinal(name));
            return selection.Any() ? Data[selection.First()] : null;
        }
    }
}

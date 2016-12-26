// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core.Diagnostics;

namespace Microsoft.R.Host.Client.API {
    public sealed class DataFrame {
        public IReadOnlyList<string> ColumnNames { get; }
        public IReadOnlyList<string> RowNames { get; }
        public IReadOnlyList<IReadOnlyList<object>> Data { get; }

        public DataFrame(IReadOnlyList<string> rowNames, IReadOnlyList<string> columnNames, IReadOnlyList<IReadOnlyList<object>> data) {
            Check.ArgumentNull(nameof(data), data);
            Check.ArgumentOutOfRange(nameof(data), () => data.Count == 0);
            Check.ArgumentOutOfRange(nameof(rowNames), () => rowNames != null && rowNames.Count != data[0].Count);
            Check.ArgumentOutOfRange(nameof(columnNames), () => columnNames != null && columnNames.Count != data.Count);
            Check.ArgumentOutOfRange(nameof(data), () => data.Select(x => x.Count != rowNames.Count).Any());

            RowNames = new List<string>(rowNames);
            ColumnNames = new List<string>(columnNames);
            Data = data;
        }
    }
}

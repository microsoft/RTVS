// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client.API {
    public sealed class RDataFrame {
        public IReadOnlyList<string> ColumnNames { get; }
        public IReadOnlyList<string> RowNames { get; }
        public IReadOnlyList<IReadOnlyList<object>> Data { get; }

        public RDataFrame(IReadOnlyList<string> rowNames, IReadOnlyList<string> columnNames, IReadOnlyList<IReadOnlyList<object>> data) {
            RowNames = new List<string>(rowNames);
            ColumnNames = new List<string>(columnNames);
            Data = data;
        }
    }
}

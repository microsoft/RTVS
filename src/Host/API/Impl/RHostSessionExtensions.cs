// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.DataInspection;
using static Microsoft.R.DataInspection.REvaluationResultProperties;

namespace Microsoft.R.Host.Client {
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

    public static class RHostSessionExtensions {
        private static readonly Dictionary<string, Type> _types = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase) {
            { "integer", typeof(int) }
        };

        public static async Task<List<object>> GetListAsync(this IRHostSession session, string expression, CancellationToken cancellationToken = default(CancellationToken)) {
            var children = await session.DescribeChildrenAsync(expression, HasChildrenProperty, null, cancellationToken);
            var list = new List<object>();
            foreach (var c in children) {
                REvaluationResult r = await session.EvaluateAsync(c.Expression, REvaluationKind.Normal);
                list.Add(r.Result.ToObject<object>());
            }
            return list;
        }

        public static async Task<RDataFrame> GetDataFrameAsync(this IRHostSession session, string expression, CancellationToken cancellationToken = default(CancellationToken)) {
            var children = await session.DescribeChildrenAsync(expression, HasChildrenProperty, null, cancellationToken);
            return new RDataFrame(new List<string>(), new List<string>(), new List<List<object>>());
        }

        public static async Task<T[,]> GetMatrixAsync<T>(this IRHostSession session, string expression, CancellationToken cancellationToken = default(CancellationToken)) {
            var children = await session.DescribeChildrenAsync(expression, HasChildrenProperty, null, cancellationToken);
            var m = new T[0, 0];
            return m;
        }

        public static Type GetValueType(this IRValueInfo vi) {
            Type t;
            return _types.TryGetValue(vi.TypeName, out t) ? t : typeof(object);
        }

        public static List<T> ToListOf<T>(this IEnumerable<object> e) {
            return new List<T>(e.Select(x => (T)Convert.ChangeType(x, typeof(T))));
        }
    }
}

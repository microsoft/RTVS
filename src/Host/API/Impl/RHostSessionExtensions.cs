// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.FormattableString;

namespace Microsoft.R.Host.Client.API {
    public static class RHostSessionExtensions {
        public static Task CreateListAsync<T>(this IRHostSession session, string name, IEnumerable<T> e, CancellationToken cancellationToken = default(CancellationToken)) {
            var rlist = e.ToRListConstructor();
            return session.ExecuteAsync(Invariant($"{name.ToRStringLiteral()} <- {rlist}"));
        }

        public static async Task CreateDataFrameAsync(this IRHostSession session, string name, DataFrame df, CancellationToken cancellationToken = default(CancellationToken)) {
            await session.ExecuteAsync(Invariant($"{name.ToRStringLiteral()} <- {df.ToRDataFrameConstructor()}"));
            if (df.RowNames != null && df.RowNames.Count > 0) {
                await session.ExecuteAsync(Invariant($"rownames({name}) <- {df.RowNames.ToRListConstructor()}"));
            }
            if (df.ColumnNames != null && df.ColumnNames.Count > 0) {
                await session.ExecuteAsync(Invariant($"colnames({name}) <- {df.ColumnNames.ToRListConstructor()}"));
            }
        }
    }
}

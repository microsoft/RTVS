// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.R.Host.Client.Host;
using static System.FormattableString;

namespace Microsoft.R.Host.Client {
    /// <summary>
    /// Additional helpers for R session
    /// </summary>
    public static class RHostSessionExtensions {
        /// <summary>
        /// Creates list of objects in R from list of .NET objects
        /// </summary>
        /// <typeparam name="T">.NET object type</typeparam>
        /// <param name="session">R session</param>
        /// <param name="name">Name of the variable to assign the R list to</param>
        /// <param name="list">List of .NET objects</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="REvaluationException" />
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        public static Task CreateListAsync<T>(this IRHostSession session, string name, IEnumerable<T> list, CancellationToken cancellationToken = default(CancellationToken)) {
            Check.ArgumentNull(nameof(session), session);
            Check.ArgumentNull(nameof(name), name);
            Check.ArgumentNull(nameof(list), list);

            var rlist = list.ToRListConstructor();
            return session.ExecuteAsync(Invariant($"{name.ToRStringLiteral()} <- {rlist}"));
        }

        /// <summary>
        /// Creates R data frame from .NET <see cref="DataFrame"/>
        /// </summary>
        /// <param name="session">R session</param>
        /// <param name="name">Name of the variable to assign the R list to</param>
        /// <param name="df">.NET data frame</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentException" />
        /// <exception cref="REvaluationException" />
        /// <exception cref="OperationCanceledException" />
        /// <exception cref="RHostDisconnectedException" />
        public static async Task CreateDataFrameAsync(this IRHostSession session, string name, DataFrame df, CancellationToken cancellationToken = default(CancellationToken)) {
            Check.ArgumentNull(nameof(session), session);
            Check.ArgumentNull(nameof(name), name);
            Check.ArgumentNull(nameof(df), df);

            await session.ExecuteAsync(Invariant($"{name.ToRStringLiteral()} <- {df.ToRDataFrameConstructor()}"));
            if (df.RowNames != null && df.RowNames.Count > 0) {
                await session.ExecuteAsync(Invariant($"rownames({name}) <- {df.RowNames.ToRListConstructor()}"));
            }
            if (df.ColumnNames != null && df.ColumnNames.Count > 0) {
                await session.ExecuteAsync(Invariant($"colnames({name}) <- {df.ColumnNames.ToRListConstructor()}"));
            }
        }

        /// <summary>
        /// Retrieves length of R object
        /// </summary>
        /// <param name="session">R session</param>
        /// <param name="expression">Expression to evaluate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Object kength</returns>
        public static async Task<long> GetLengthAsync(this IRHostSession session, string expression, CancellationToken cancellationToken = default(CancellationToken)) {
            var info = await session.GetInformationAsync(expression, cancellationToken);
            return info.Length;
        }

        /// <summary>
        /// Retrieves R type name for an object
        /// </summary>
        /// <param name="session">R session</param>
        /// <param name="expression">Expression to evaluate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>R type name</returns>
        public static async Task<string> GetTypeNameAsync(this IRHostSession session, string expression, CancellationToken cancellationToken = default(CancellationToken)) {
            var info = await session.GetInformationAsync(expression, cancellationToken);
            return info.TypeName;
        }
    }
}

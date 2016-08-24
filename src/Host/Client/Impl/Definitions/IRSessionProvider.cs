// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Client.Host;

namespace Microsoft.R.Host.Client {
    public interface IRSessionProvider : IDisposable {
        IRSession GetOrCreate(Guid guid, IRHostBrokerConnector brokerConnector);
        IEnumerable<IRSession> GetSessions();

        /// <summary>
        /// Creates <see cref="IRSessionEvaluation"/> for R expressions to be evaluated
        /// Expressions are evaluated in a separate <see cref="IRSession"/>, no artifacts will be preserved after evaluation
        /// </summary>
        /// <param name="hostFactory"></param>
        /// <param name="startupInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IRSessionEvaluation> BeginEvaluationAsync(IRHostBrokerConnector hostFactory, RHostStartupInfo startupInfo, CancellationToken cancellationToken = default(CancellationToken));
    }
}
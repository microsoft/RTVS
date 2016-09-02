// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    public interface IRSessionProvider : IDisposable {
        Uri BrokerUri { get; }

        event EventHandler BrokerChanged;

        IRSession GetOrCreate(Guid guid);
        IEnumerable<IRSession> GetSessions();

        /// <summary>
        /// Creates <see cref="IRSessionEvaluation"/> for R expressions to be evaluated
        /// Expressions are evaluated in a separate <see cref="IRSession"/>, no artifacts will be preserved after evaluation
        /// </summary>
        /// <param name="hostFactory"></param>
        /// <param name="startupInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<IRSessionEvaluation> BeginEvaluationAsync(RHostStartupInfo startupInfo, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Name of the broker. Will be displayed in REPL.</param>
        /// <param name="path">Either a local path to the R binary or a URL to the broker.</param>
        Task<bool> TrySwitchBroker(string name, string path = null);
    }
}
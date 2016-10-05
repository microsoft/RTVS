// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Threading;

namespace Microsoft.R.Host.Client {
    public interface IRSessionSwitchBrokerTransaction : IDisposable {
        /// <summary>
        /// First step of the transaction. Acquires lock that prevents switch, reconnect 
        /// and other types of initialization from being called concurrently.
        /// </summary>
        Task AcquireLockAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Second step of the transaction. Creates connection to the broker based on the RSession parameters. 
        /// At the end of stage, old connection still exists and all pending interactions/evaluations are alive.
        /// Canceling transaction during or at the end of this stage allows to fallback to the old broker and avoid restarting session
        /// </summary>
        Task ConnectToNewBrokerAsync(CancellationToken cancellationToken, ReentrancyToken reentrancyToken);

        /// <summary>
        /// Third step of the transaction. Assumes that connection to the new broker has been established.
        /// During this stage, all existing interactions/evaluations are canceled and session is restarted using new connection.
        /// Canceling transaction during this stage will leave session in a broken stage, so manual restart is required (restarted session will be connected to the new broker).
        /// </summary>
        Task CompleteSwitchingBrokerAsync(CancellationToken cancellationToken);
    }
}
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    public interface IRSessionReconnectTransaction : IDisposable {
        /// <summary>
        /// First step of the transaction. Acquires lock that prevents switch, reconnect 
        /// and other types of initialization from being called concurrently.
        /// </summary>
        Task AcquireLockAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Second step of the transaction. Performs actual reconnection.
        /// </summary>
        Task ReconnectAsync(CancellationToken cancellationToken);
    }
}
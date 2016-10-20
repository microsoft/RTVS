// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Threading;

namespace Microsoft.R.Host.Client {
    public interface IRSessionReconnectTransaction : IRSessionTransaction {
        /// <summary>
        /// Second step of the transaction. Performs actual reconnection.
        /// </summary>
        Task ReconnectAsync(CancellationToken cancellationToken, ReentrancyToken reentrancyToken);
    }
}
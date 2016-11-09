// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Client.Host;

namespace Microsoft.R.Host.Client {
    public interface IRSessionProvider : IDisposable {
        event EventHandler BrokerChanging;
        event EventHandler BrokerChangeFailed;
        event EventHandler BrokerChanged;
        event EventHandler<BrokerStateChangedEventArgs> BrokerStateChanged;

        bool IsConnected { get; }
        IBrokerClient Broker { get; }

        IRSession GetOrCreate(Guid guid);
        IEnumerable<IRSession> GetSessions();

        /// <summary>
        /// Tests connection to the broker without changing current one.
        /// </summary>
        /// <param name="name">Name of the broker. Will be displayed in REPL.</param>
        /// <param name="path">Either a local path to the R binary or a URL to the broker.</param>
        /// <param name="cancellationToken"></param>
        Task TestBrokerConnectionAsync(string name, string path, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Name of the broker. Will be displayed in REPL.</param>
        /// <param name="path">Either a local path to the R binary or a URL to the broker.</param>
        /// <param name="cancellationToken"></param>
        Task<bool> TrySwitchBrokerAsync(string name, string path = null, CancellationToken cancellationToken = default(CancellationToken));
    }
}
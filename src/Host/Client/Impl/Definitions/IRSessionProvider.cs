// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Security;
using Microsoft.R.Host.Client.Host;

namespace Microsoft.R.Host.Client {
    public interface IRSessionProvider : IDisposable {
        event EventHandler BrokerChanging;
        event EventHandler BrokerChangeFailed;
        event EventHandler BrokerChanged;
        event EventHandler<BrokerStateChangedEventArgs> BrokerStateChanged;
        event EventHandler<HostLoadChangedEventArgs> HostLoadChanged;
        event EventHandler BeforeDisposed;

        bool HasBroker { get; }
        bool IsConnected { get; }
        IBrokerClient Broker { get; }

        IRSession GetOrCreate(string sessionId);
        IEnumerable<IRSession> GetSessions();

        /// <summary>
        /// Tests connection to the broker without changing current one.
        /// </summary>
        /// <param name="name">Name of the broker. Will be displayed in REPL.</param>
        /// <param name="connectionInfo">Either a local path to the R binary or a URL to the broker + broker connection parameters</param>
        /// <param name="cancellationToken"></param>
        Task TestBrokerConnectionAsync(string name, BrokerConnectionInfo connectionInfo = default(BrokerConnectionInfo), CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Name of the broker. Will be displayed in REPL.</param>
        /// <param name="connectionInfo">Either a local path to the R binary or a URL to the broker + broker connection parameters</param>
        /// <param name="cancellationToken"></param>
        Task<bool> TrySwitchBrokerAsync(string name, BrokerConnectionInfo connectionInfo = default(BrokerConnectionInfo), CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Removes current broker, switching all sessions to the disconnected state
        /// </summary>
        /// <param name="cancellationToken"></param>
        Task RemoveBrokerAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
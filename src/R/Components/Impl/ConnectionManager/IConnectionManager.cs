// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Components.ConnectionManager {
    public interface IConnectionManager : IDisposable {
        bool IsConnected { get; }
        bool IsRunning { get; }
        IConnection ActiveConnection { get; }

        /// <summary>
        /// Represent saved connections in the order of usage (latest first)
        /// </summary>
        ReadOnlyCollection<IConnection> RecentConnections { get; }

        event EventHandler RecentConnectionsChanged;
        event EventHandler ConnectionStateChanged;

        IConnection AddOrUpdateConnection(IConnectionInfo connectionInfo);
        IConnection GetOrAddConnection(string name, string path, string rCommandLineArguments, bool isUserCreated);
        IConnection GetConnection(string name);

        /// <summary>
        /// Tries to remove connection.
        /// </summary>
        /// <param name="name">Name of the connection</param>
        /// <returns>True if connection is actually removed by this call. False if connection is removed already or if it is active connection.</returns>
        bool TryRemove(string name);

        /// <summary>
        /// Disconnects all sessions from active connection and switches to disconnected state
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task DisconnectAsync(CancellationToken cancellationToken = default(CancellationToken));

        Task ConnectAsync(IConnectionInfo connection, CancellationToken cancellationToken = default(CancellationToken));
        Task ReconnectAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task TestConnectionAsync(IConnectionInfo connection, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Tries to connect to the connection that was last used during previous work session
        /// Won't do anything if <see cref="ConnectAsync"/> or <see cref="ReconnectAsync"/> was called for the same instance.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> TryConnectToPreviouslyUsedAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}

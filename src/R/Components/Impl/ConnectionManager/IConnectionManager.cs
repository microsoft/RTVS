// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Components.ConnectionManager {
    public interface IConnectionManager : IDisposable {
        IConnectionManagerVisualComponent GetOrCreateVisualComponent(int id = 0);

        bool IsConnected { get; }
        IConnection ActiveConnection { get; }

        /// <summary>
        /// Represent saved connections in the order of usage (latest first)
        /// </summary>
        ReadOnlyCollection<IConnection> RecentConnections { get; }

        event EventHandler RecentConnectionsChanged;
        event EventHandler<ConnectionEventArgs> ConnectionStateChanged;

        IConnection AddOrUpdateConnection(string name, string path, string rCommandLineArguments, bool isUserCreated);
        IConnection GetOrAddConnection(string name, string path, string rCommandLineArguments, bool isUserCreated);

        /// <summary>
        /// Tries to remove connection.
        /// </summary>
        /// <param name="name">Name of the connection</param>
        /// <returns>True if connection is actually removed by this call. False if connection is removed already or if it is active connection.</returns>
        bool TryRemove(string name);

        /// <summary>
        /// Removes connection. If connection is active, disconnects all sessions from it.
        /// </summary>
        /// <param name="connectionName">Name of the connection</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task RemoveAsync(string connectionName, CancellationToken cancellationToken = default(CancellationToken));

        Task ConnectAsync(IConnectionInfo connection, CancellationToken cancellationToken = default(CancellationToken));
        Task ReconnectAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task TestConnectionAsync(IConnectionInfo connection, CancellationToken cancellationToken = default(CancellationToken));
        Task<bool> TryConnectToPreviouslyUsedAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}

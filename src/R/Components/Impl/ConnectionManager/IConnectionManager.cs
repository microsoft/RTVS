// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Microsoft.R.Components.ConnectionManager {
    public interface IConnectionManager : IDisposable {
        IConnectionManagerVisualComponent GetOrCreateVisualComponent(IConnectionManagerVisualComponentContainerFactory value, int id);

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
        bool TryRemove(Uri id);

        Task ConnectAsync(IConnectionInfo connection);
        Task TestConnectionAsync(IConnectionInfo connection);
    }
}

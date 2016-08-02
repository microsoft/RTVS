// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;

namespace Microsoft.R.Components.ConnectionManager {
    public interface IConnectionManager : IDisposable {
        bool IsConnected { get; }
        IConnection ActiveConnection { get; }
        ReadOnlyCollection<IConnection> RecentConnections { get; }

        event EventHandler RecentConnectionsChanged;
        event EventHandler<ConnectionEventArgs> ConnectionStateChanged;

        void AddOrUpdateLocalConnection(string name, string rBasePath);
    }
}

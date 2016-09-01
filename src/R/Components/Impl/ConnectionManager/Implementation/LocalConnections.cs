// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Generic;

namespace Microsoft.R.Components.ConnectionManager.Implementation {
    /// <summary>
    /// Provides information on locally installed interpreters
    /// </summary>
    internal sealed class LocalConnections : IEnumerable<IConnection> {
        private readonly List<IConnection> _connections = new List<IConnection>();

        public LocalConnections() {

        }

        public IEnumerator<IConnection> GetEnumerator() => _connections.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _connections.GetEnumerator();
    }
}

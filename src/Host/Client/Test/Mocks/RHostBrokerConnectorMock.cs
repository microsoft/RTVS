// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Client.Host;

namespace Microsoft.R.Host.Client.Test.Mocks {
    public sealed class RHostBrokerConnectorMock : IRHostBrokerConnector {
        public void Dispose() {
        }

        public Task<RHost> Connect(string name, IRCallbacks callbacks, string rCommandLineArguments = null, int timeout = 3000, CancellationToken cancellationToken = new CancellationToken()) {
            throw new System.NotImplementedException();
        }

        public Uri BrokerUri { get; private set; }
        public event EventHandler BrokerChanged;

        public void SwitchToLocalBroker(string rBasePath, string rHostDirectory = null) {
            BrokerUri = new Uri(rBasePath);
            BrokerChanged?.Invoke(this, new EventArgs());
        }
    }
}
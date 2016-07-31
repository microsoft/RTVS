// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Client.Host;

namespace Microsoft.R.Host.Client.Test.Mocks {
    public sealed class RHostBrokerConnectorMock : IRHostBrokerConnector {
        public Task<RHost> ConnectToRHost(string name, IRCallbacks callbacks, string rCommandLineArguments = null, int timeout = 3000, CancellationToken cancellationToken = new CancellationToken()) {
            throw new System.NotImplementedException();
        }

        public string BrokerId { get; private set; }
        public event EventHandler BrokerIdChanged;

        public void SwitchToLocalBroker(string rBasePath, string rHostDirectory = null) {
            BrokerId = rBasePath;
            BrokerIdChanged?.Invoke(this, new EventArgs());
        }
    }
}
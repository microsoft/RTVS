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

        public Task<RHost> ConnectAsync(string name, IRCallbacks callbacks, int timeout = 3000, CancellationToken cancellationToken = new CancellationToken()) {
            throw new System.NotImplementedException();
        }

        public bool IsRemote { get; private set; }
        public Uri BrokerUri { get; private set; }
        public event EventHandler BrokerChanged;

        public void SwitchToLocalBroker(string name, string rBasePath = null, string rCommandLineArguments = null, string rHostDirectory = null) {
            BrokerUri = rBasePath != null ? new Uri(rBasePath) : new Uri(@"C:\");
            IsRemote = false;
            BrokerChanged?.Invoke(this, new EventArgs());
        }

        public void SwitchToRemoteBroker(Uri uri, string rCommandLineArguments = null) {
            BrokerUri = uri;
            IsRemote = true;
            BrokerChanged?.Invoke(this, new EventArgs());
        }
    }
}
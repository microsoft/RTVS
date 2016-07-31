// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Interpreters;

namespace Microsoft.R.Host.Client.Host {
    public sealed class RHostBrokerConnector : IRHostBrokerConnector {
        private volatile IRHostConnector _hostConnector;

        public string BrokerId { get; private set; }

        public event EventHandler BrokerIdChanged;

        public RHostBrokerConnector() {
            SwitchToLocalBroker(null);
        }

        public void SwitchToLocalBroker(string rBasePath, string rHostDirectory = null) {
            _hostConnector = new LocalRHostConnector(new RInstallation().GetRInstallPath(rBasePath, new SupportedRVersionRange()), rHostDirectory);
            BrokerId = rBasePath;
            BrokerIdChanged?.Invoke(this, new EventArgs());
        }

        public Task<RHost> ConnectToRHost(string name, IRCallbacks callbacks, string rCommandLineArguments = null, int timeout = 3000, CancellationToken cancellationToken = new CancellationToken())
            => _hostConnector.ConnectToRHost(name, callbacks, rCommandLineArguments, timeout, cancellationToken);
    }
}
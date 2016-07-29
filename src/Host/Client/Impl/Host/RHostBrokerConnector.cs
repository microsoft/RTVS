// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Interpreters;

namespace Microsoft.R.Host.Client.Host {
    public sealed class RHostBrokerConnector : IRHostBrokerConnector {
        private readonly string _name;
        private volatile IRHostConnector _hostConnector;

        public Uri BrokerUri { get; private set; }

        public event EventHandler BrokerChanged;

        public RHostBrokerConnector(Uri brokerUri = null, string name = null) {
            _name = name;

            if (brokerUri == null) {
                SwitchToLocalBroker(null);
            } else {
                BrokerUri = brokerUri;
                _hostConnector = new RemoteRHostConnector(brokerUri);
            }
        }

        public void Dispose() {
            _hostConnector?.Dispose();
        }

        public void SwitchToLocalBroker(string rBasePath, string rHostDirectory = null) {
            _hostConnector?.Dispose();

            var installPath = new RInstallation().GetRInstallPath(rBasePath, new SupportedRVersionRange());

            _hostConnector = new LocalRHostConnector(_name, installPath, rHostDirectory);
            BrokerUri = new Uri(installPath);
            BrokerChanged?.Invoke(this, new EventArgs());
        }

        public Task<RHost> Connect(string name, IRCallbacks callbacks, string rCommandLineArguments = null, int timeout = 3000, CancellationToken cancellationToken = new CancellationToken())
            => _hostConnector.Connect(name, callbacks, rCommandLineArguments, timeout, cancellationToken);
    }
}
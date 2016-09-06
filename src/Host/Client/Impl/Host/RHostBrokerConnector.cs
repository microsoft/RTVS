// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Interpreters;

namespace Microsoft.R.Host.Client.Host {
    public sealed class RHostBrokerConnector : IRHostBrokerConnector {
        private readonly ICoreShell _coreShell;
        private IRHostConnector _hostConnector;

        public Uri BrokerUri => _hostConnector.BrokerUri;

        public bool IsRemote => _hostConnector.IsRemote;

        public event EventHandler BrokerChanged;
        public RHostBrokerConnector(ICoreShell coreShell = null) {
            _coreShell = coreShell;
            _hostConnector = new NullRHostConnector();
        }

        public void Dispose() {
            _hostConnector.Dispose();
        }

        public void SwitchToLocalBroker(string name, string rBasePath = null, string rHostDirectory = null) {
            var installPath = rBasePath ?? new RInstallation().GetCompatibleEngines().FirstOrDefault()?.InstallPath;
            if (!string.IsNullOrEmpty(installPath)) {
                var newConnector = new LocalRHostConnector(name, installPath, rHostDirectory);
                var oldConnector = Interlocked.Exchange(ref _hostConnector, newConnector);

                oldConnector.Dispose();
                BrokerChanged?.Invoke(this, new EventArgs());
            }
        }

        public void SwitchToRemoteBroker(Uri uri) {
            var oldConnector = Interlocked.Exchange(ref _hostConnector, new RemoteRHostConnector(uri, _coreShell));
            oldConnector.Dispose();

            BrokerChanged?.Invoke(this, new EventArgs());
        }

        public Task<RHost> ConnectAsync(string name, IRCallbacks callbacks, string rCommandLineArguments, int timeout = 3000, CancellationToken cancellationToken = new CancellationToken())
            => _hostConnector.ConnectAsync(name, callbacks, rCommandLineArguments, timeout, cancellationToken);
    }
}
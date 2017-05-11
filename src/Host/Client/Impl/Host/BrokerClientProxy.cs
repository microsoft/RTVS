// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client.Host {
    internal sealed class BrokerClientProxy : IBrokerClient {
        private IBrokerClient _broker;

        public BrokerClientProxy() {
            _broker = new NullBrokerClient();
        }

        public IBrokerClient Set(IBrokerClient broker) {
            return Interlocked.Exchange(ref _broker, broker);
        }

        public void Dispose() {
            var broker = Interlocked.Exchange(ref _broker, new NullBrokerClient());
            broker.Dispose();
        }

        public BrokerConnectionInfo ConnectionInfo => _broker.ConnectionInfo;
        public string Name => _broker.Name;
        public bool IsRemote => _broker.IsRemote;
        public bool IsVerified => _broker.IsVerified;
        public bool HasBroker => !(_broker is NullBrokerClient);

        public Task<T> GetHostInformationAsync<T>(CancellationToken cancellationToken) => _broker.GetHostInformationAsync<T>(cancellationToken);

        public Task<RHost> ConnectAsync(HostConnectionInfo connectionInfo, CancellationToken cancellationToken = default(CancellationToken)) 
            => _broker.ConnectAsync(connectionInfo, cancellationToken);

        public Task TerminateSessionAsync(string name, CancellationToken cancellationToken = default(CancellationToken))
            => _broker.TerminateSessionAsync(name, cancellationToken);

        public Task<string> HandleUrlAsync(string url, CancellationToken cancellationToken) => _broker.HandleUrlAsync(url, cancellationToken);

        public Task DeleteProfileAsync(CancellationToken cancellationToken) => _broker.DeleteProfileAsync(cancellationToken);
    }
}
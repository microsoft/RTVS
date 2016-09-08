// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
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
            var broker = Interlocked.Exchange(ref _broker, null);
            broker.Dispose();
        }

        public string Name => _broker.Name;
        public bool IsRemote => _broker.IsRemote;
        public Uri Uri => _broker.Uri;

        public Task<RHost> ConnectAsync(string name, IRCallbacks callbacks, string rCommandLineArguments = null, int timeout = 3000, CancellationToken cancellationToken = new CancellationToken())
            => _broker.ConnectAsync(name, callbacks, rCommandLineArguments, timeout, cancellationToken);
    }
}
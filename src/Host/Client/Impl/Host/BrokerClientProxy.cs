// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Client.Host {
    internal sealed class BrokerClientProxy : IBrokerClient {
        private readonly AsyncReaderWriterLock _connectArwl;
        private IBrokerClient _broker;

        public BrokerClientProxy(AsyncReaderWriterLock connectArwl) {
            _broker = new NullBrokerClient();
            _connectArwl = connectArwl;
        }

        public IBrokerClient Set(IBrokerClient broker) {
            return Interlocked.Exchange(ref _broker, broker);
        }

        public void Dispose() {
            var broker = Interlocked.Exchange(ref _broker, new NullBrokerClient());
            broker.Dispose();
        }

        public string Name => _broker.Name;
        public bool IsRemote => _broker.IsRemote;
        public Uri Uri => _broker.Uri;
        public AboutHost AboutHost => _broker?.AboutHost;
        public bool IsVerified => _broker.IsVerified;

        public Task PingAsync() => _broker.PingAsync();

        public async Task<RHost> ConnectAsync(string name, IRCallbacks callbacks, string rCommandLineArguments = null, int timeout = 3000,
            CancellationToken cancellationToken = default(CancellationToken), ReentrancyToken reentrancyToken = default(ReentrancyToken)) {

            using (await _connectArwl.ReaderLockAsync(cancellationToken, reentrancyToken)) {
                return await _broker.ConnectAsync(name, callbacks, rCommandLineArguments, timeout, cancellationToken);
            }
        }

        public Task TerminateSessionAsync(string name, CancellationToken cancellationToken = new CancellationToken()) =>
            _broker.TerminateSessionAsync(name, cancellationToken);

        public string HandleUrl(string url, CancellationToken ct) => _broker.HandleUrl(url, ct);
    }
}
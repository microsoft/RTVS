// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;

namespace Microsoft.R.Host.Client.Host {
    public sealed class NullBrokerClient : IBrokerClient {
        private static Task<RHost> Result { get; } = TaskUtilities.CreateCanceled<RHost>(
            new RHostDisconnectedException(Resources.RHostDisconnected));

        public BrokerConnectionInfo ConnectionInfo { get; } = default(BrokerConnectionInfo);
        public string Name { get; } = string.Empty;
        public bool IsRemote { get; } = true;
        public bool IsVerified { get; } = true;

        public Task<T> GetHostInformationAsync<T>(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(default(T));
        public Task PingAsync() => Result;

        public Task<RHost> ConnectAsync(HostConnectionInfo connectionInfo, CancellationToken cancellationToken = default(CancellationToken)) => Result;

        public Task TerminateSessionAsync(string name, CancellationToken cancellationToken = new CancellationToken()) => Result;

        public void Dispose() { }

        public Task<string> HandleUrlAsync(string url, CancellationToken cancellationToken) => Task.FromResult(url);

        public Task DeleteProfileAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
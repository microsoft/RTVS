// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Client.Host {
    internal sealed class NullBrokerClient : IBrokerClient {
        private static Task<RHost> Result { get; } = TaskUtilities.CreateCanceled<RHost>(
            new RHostDisconnectedException(Resources.RHostDisconnected));

        public Uri Uri { get; } = new Uri("http://localhost");
        public string Name { get; } = string.Empty;
        public bool IsRemote { get; } = true;
        public bool IsVerified => true;

        public Task<T> GetHostInformationAsync<T>(CancellationToken cancellationToken = default(CancellationToken)) => Task.FromResult(default(T));
        public Task PingAsync() => Result;

        public Task<RHost> ConnectAsync(BrokerConnectionInfo connectionInfo, CancellationToken cancellationToken = default(CancellationToken)) => Result;

        public Task TerminateSessionAsync(string name, CancellationToken cancellationToken = new CancellationToken()) => Result;

        public void Dispose() { }

        public Task<string> HandleUrlAsync(string url, CancellationToken cancellationToken) => Task.FromResult(url);

        public Task<bool> DeleteRemoteUserProfileAsync(CancellationToken cancellationToken) => Task.FromResult(false);
    }
}
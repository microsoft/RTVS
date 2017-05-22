// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client.Host {
    public interface IBrokerClient : IDisposable {
        BrokerConnectionInfo ConnectionInfo { get; }
        string Name { get; }
        bool IsRemote { get; }
        bool IsVerified { get; }

        Task<RHost> ConnectAsync(HostConnectionInfo connectionInfo, CancellationToken cancellationToken = default(CancellationToken));
        Task TerminateSessionAsync(string name, CancellationToken cancellationToken = default(CancellationToken));
        Task<string> HandleUrlAsync(string url, CancellationToken cancellationToken = default(CancellationToken));
        Task<T> GetHostInformationAsync<T>(CancellationToken cancellationToken = default(CancellationToken));
        Task DeleteProfileAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
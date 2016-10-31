// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Client.Host {
    public interface IBrokerClient : IDisposable {
        string Name { get; }
        bool IsRemote { get; }
        Uri Uri { get; }
        bool IsVerified { get; }

        Task<AboutHost> GetHostInformationAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task PingAsync();
        Task<RHost> ConnectAsync(BrokerConnectionInfo connectionInfo, CancellationToken cancellationToken = default(CancellationToken));
        Task TerminateSessionAsync(string name, CancellationToken cancellationToken = default(CancellationToken));
        Task<string> HandleUrlAsync(string url, CancellationToken cancellationToken);
    }
}
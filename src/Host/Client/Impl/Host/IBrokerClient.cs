// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client.Host {
    public interface IBrokerClient : IDisposable {
        string Name { get; }
        bool IsRemote { get; }
        Uri Uri { get; }
        Task<RHost> ConnectAsync(string name, IRCallbacks callbacks, string rCommandLineArguments = null, int timeout = 3000, CancellationToken cancellationToken = default(CancellationToken));
        string HandleUrl(string url, CancellationToken ct);
    }
}
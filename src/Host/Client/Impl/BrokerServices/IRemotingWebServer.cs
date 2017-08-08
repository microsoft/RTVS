// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;

namespace Microsoft.R.Host.Client {
    public interface IRemotingWebServer {
        Task<string> HandleRemoteWebUrlAsync(string remoteUrl, string baseAddress, string name, IConsole console, CancellationToken ct = default(CancellationToken));
        Task<string> HandleLocalStaticFileUrlAsync(string url, IConsole console, CancellationToken ct = default(CancellationToken));
        Task<string> HandleRemoteStaticFileUrlAsync(string url, IRSessionProvider rSessionProvider, IConsole console, CancellationToken ct = default(CancellationToken));
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;

namespace Microsoft.R.Host.Client {
    public interface IRemotingWebServer {
        Task<string> CreateWebServerAsync(string remoteUrl, string baseAddress, string name, IActionLog log, IConsole console, CancellationToken ct = default(CancellationToken));
        Task<string> CreateLocalStaticFileServerAsync(string url, IFileSystem fs, IActionLog log, IConsole console, CancellationToken ct = default(CancellationToken));
        Task<string> CreateRemoteStaticFileServerAsync(string url, IRSessionProvider rSessionProvider, IFileSystem fs, IActionLog log, IConsole console, CancellationToken ct = default(CancellationToken));
    }
}

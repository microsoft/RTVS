// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core.Logging;
using Microsoft.R.Host.Client.BrokerServices;

namespace Microsoft.R.Host.Client {
    public class RemotingWebServer : IRemotingWebServer {
        public Task<string> CreateWebServerAsync(string remoteUrl, string baseAddress, string name, IActionLog log, IConsole console, CancellationToken ct = default(CancellationToken)) {
            return WebServer.CreateWebServerAsync(remoteUrl, baseAddress, name, log, console, ct);
        }
    }
}

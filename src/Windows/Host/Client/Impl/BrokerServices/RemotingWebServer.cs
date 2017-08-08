// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Services;
using Microsoft.R.Host.Client.BrokerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    public class RemotingWebServer : IRemotingWebServer {
        private readonly IServiceContainer _services;
        private readonly IFileSystem _fs;
        private readonly IActionLog _log;

        private LocalStaticFileServer _localStaticFileServer;
        private object _localStaticFileServerLock = new object();

        private RemoteStaticFileServer _remoteStaticFileServer;
        private object _remoteStaticFileServerLock = new object();

        public RemotingWebServer(IServiceContainer services) {
            _services = services;
            _fs = _services.FileSystem();
            _log = _services.Log();
        }

        public Task<string> HandleRemoteWebUrlAsync(string remoteUrl, string baseAddress, string name, IConsole console, CancellationToken ct = default(CancellationToken)) {
            return WebServer.CreateWebServerAndHandleUrlAsync(remoteUrl, baseAddress, name, _log, console, ct);
        }

        public Task<string> HandleLocalStaticFileUrlAsync(string url, IConsole console, CancellationToken ct = default(CancellationToken)) {
            CreateLocalStaticFileServer(console);
            return _localStaticFileServer.HandleUrlAsync(url, ct);
        }

        public Task<string> HandleRemoteStaticFileUrlAsync(string url,  IRSessionProvider rSessionProvider, IConsole console, CancellationToken ct = default(CancellationToken)) {
            CreateRemoteStaticFileServerAsync(rSessionProvider, console);
            return _remoteStaticFileServer.HandleUrlAsync(url, ct);
        }

        private void CreateLocalStaticFileServer(IConsole console) {
            lock (_localStaticFileServerLock) {
                _localStaticFileServer = _localStaticFileServer ?? new LocalStaticFileServer(_fs, _log, console);
            }
        }

        private void CreateRemoteStaticFileServerAsync(IRSessionProvider rSessionProvider, IConsole console) {
            lock (_remoteStaticFileServerLock) {
                _remoteStaticFileServer = _remoteStaticFileServer ?? new RemoteStaticFileServer(rSessionProvider, _fs, _log, console);
            }
        }
    }
}

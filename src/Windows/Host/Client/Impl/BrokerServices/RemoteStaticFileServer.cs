// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;
using Microsoft.R.Host.Client.Session;

namespace Microsoft.R.Host.Client.BrokerServices {
    public class RemoteStaticFileServer: StaticFileServerBase {
        private readonly IRSessionProvider _sessionProvider;
        private const string remoteFileSession = "FileFetcher";
        private static RemoteStaticFileServer _server;
        private static object _serverLock = new object();
        private IRSession _session;

        public RemoteStaticFileServer(IRSessionProvider sessionProvider, IFileSystem fs, IActionLog log, IConsole console) : base(fs, log, console) {
            _sessionProvider = sessionProvider;
        }

        public async Task<string> HandleUrlAsync(string urlStr, CancellationToken ct = default(CancellationToken)) {
            await InitializeAsync(ct);

            IRSession session = _sessionProvider.GetOrCreate(remoteFileSession);
            await session.EnsureHostStartedAsync(new RHostStartupInfo(), null, 3000, ct);
            _session = session;

            Log.WriteLine(LogVerbosity.Minimal, MessageCategory.General, Resources.Info_StaticFileServerStarted.FormatInvariant(Listener.Prefixes.FirstOrDefault()));
            Console.WriteLine(Resources.Info_StaticFileServerStarted.FormatInvariant(Listener.Prefixes.FirstOrDefault()));

            string path = urlStr;
            if (urlStr.StartsWithIgnoreCase("file://")) {
                UriBuilder ub = new UriBuilder(urlStr);
                path = ub.Path;
            }
            
            return GetFileServerPath(path);
        }

        public static Task<string> CreateAsync(string url, IRSessionProvider sp, IFileSystem fs, IActionLog log, IConsole console, CancellationToken ct = default(CancellationToken)) {
            lock (_serverLock) {
                _server = _server ?? new RemoteStaticFileServer(sp, fs, log, console);
            }

            return _server.HandleUrlAsync(url);
        }


        public override async Task HandleRequestAsync(HttpListenerContext context, CancellationToken ct) {
            if (!_session.IsHostRunning) {
                await _session.EnsureHostStartedAsync(new RHostStartupInfo(), null, 3000, ct);
            }

            var uri = context.Request.Url.PathAndQuery;
            if (await _session.FileExistsAsync(uri)) {
                await CopyFileAsync(uri, context, ct);
                return;
            }

            if (uri.StartsWithIgnoreCase("/")) {
                uri =  uri.Substring(1);
                await CopyFileAsync(uri, context, ct);
                return;
            }
        }

        private async Task CopyFileAsync(string path, HttpListenerContext context, CancellationToken ct) {
            using (DataTransferSession dts = new DataTransferSession(_session, FileSystem)) {
                await dts.CopyToFileStreamAsync(path, context.Response.OutputStream, true, null, ct);
            }
            context.Response.OutputStream.Close();
        }
    }
}

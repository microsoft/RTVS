// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Logging;

namespace Microsoft.R.Host.Client.BrokerServices {
    public class LocalStaticFileServer : StaticFileServerBase {
        private static LocalStaticFileServer _server;
        private static object _serverLock = new object();

        public LocalStaticFileServer(IFileSystem fs, IActionLog log, IConsole console) : base(fs, log, console) {
        }

        public async Task<string> HandleUrlAsync(string urlStr, CancellationToken ct = default(CancellationToken)) {
            await InitializeAsync(ct);

            Log.WriteLine(LogVerbosity.Minimal, MessageCategory.General, Resources.Info_StaticFileServerStarted.FormatInvariant(Listener.Prefixes.FirstOrDefault()));
            Console.WriteLine(Resources.Info_StaticFileServerStarted.FormatInvariant(Listener.Prefixes.FirstOrDefault()));

            UriBuilder ub = new UriBuilder(urlStr);
            return GetFileServerPath(ub.Path);
        }

        public static Task<string> CreateAsync(string url, IFileSystem fs, IActionLog log, IConsole console, CancellationToken ct = default(CancellationToken)) {
            lock (_serverLock) {
                _server = _server ?? new LocalStaticFileServer(fs, log, console);
            }

            return _server.HandleUrlAsync(url);
        }

        public override async Task HandleRequestAsync(HttpListenerContext context, CancellationToken ct) {
            var uri = context.Request.Url.PathAndQuery;
            if (!FileSystem.FileExists(uri) && uri.StartsWith("/")) {
                uri = uri.Substring(1);
            }

            if (FileSystem.FileExists(uri)) {
                using (var stream = FileSystem.FileOpen(uri, FileMode.Open)) {
                    await stream.CopyToAsync(context.Response.OutputStream, null, ct);
                }
                context.Response.OutputStream.Close();
            }
        }
    }
}

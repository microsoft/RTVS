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
    internal class LocalStaticFileServer : StaticFileServerBase {
        public LocalStaticFileServer(IFileSystem fs, IActionLog log, IConsole console) : base(fs, log, console) {
        }

        public async Task<string> HandleUrlAsync(string urlStr, CancellationToken ct = default(CancellationToken)) {
            await InitializeAsync(ct);

            Log.WriteLine(LogVerbosity.Minimal, MessageCategory.General, Resources.Info_StaticFileServerStarted.FormatInvariant(Listener.Prefixes.FirstOrDefault()));
            Console.WriteLine(Resources.Info_StaticFileServerStarted.FormatInvariant(Listener.Prefixes.FirstOrDefault()));

            return GetFileServerPath(urlStr);
        }

        public override async Task HandleRequestAsync(HttpListenerContext context, CancellationToken ct) {
            UriBuilder ub = new UriBuilder();
            ub.Scheme = "file";
            ub.Host = "";
            ub.Path = context.Request.Url.LocalPath;
            ub.Query = context.Request.Url.Query;

            var uri = ub.Uri.LocalPath;
            if (FileSystem.FileExists(uri)) {
                using (var stream = FileSystem.FileOpen(uri, FileMode.Open)) {
                    await stream.CopyToAsync(context.Response.OutputStream, null, ct);
                }
                context.Response.OutputStream.Close();
            }
        }
    }
}

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
using static System.FormattableString;

namespace Microsoft.R.Host.Client.BrokerServices {
    internal abstract class StaticFileServerBase {
        private static Random _random = new Random();
        private HttpListener _listener;

        protected IFileSystem FileSystem { get; }
        protected IActionLog Log { get; }
        protected IConsole Console { get; }
        protected HttpListener Listener => _listener;

        public StaticFileServerBase(IFileSystem fs, IActionLog log, IConsole console) {
            FileSystem = fs;
            Log = log;
            Console = console;
        }

        protected async Task<bool> InitializeAsync(CancellationToken ct) {
            await TaskUtilities.SwitchToBackgroundThread();

            _listener = _listener ?? new HttpListener();
            if (_listener != null && _listener.IsListening) {
                return true;
            }

            while (true) {
                if (ct.IsCancellationRequested) {
                    return false;
                }
                _listener.Prefixes.Clear();
                var lport = _random.GetEphemeralPort();
                _listener.Prefixes.Add(Invariant($"http://{IPAddress.Loopback.ToString()}:{lport}/"));
                try {
                    _listener.Start();
                } catch (HttpListenerException) {
                    _listener.Close();
                    continue;
                }
                break;
            }

            DoWorkAsync(ct).DoNotWait();
            return true;
        }


        private async Task DoWorkAsync(CancellationToken ct = default(CancellationToken)) {
            try {
                while (_listener.IsListening) {
                    if (ct.IsCancellationRequested) {
                        _listener.Stop();
                        break;
                    }

                    HttpListenerContext context = await _listener.GetContextAsync();
                    await HandleRequestAsync(context, ct);
                }
            } catch (Exception ex) when (!ex.IsCriticalException()) {
                Log.WriteLine(LogVerbosity.Minimal, MessageCategory.Error, Resources.Error_StaticFileServerStopped.FormatInvariant(ex.Message));
                Console.WriteErrorLine(Resources.Error_StaticFileServerStopped.FormatInvariant(ex.Message));
            } finally {
                StopServer();
            }
        }

        protected void StopServer() {
            try {
                _listener.Stop();
            } catch (Exception ex) when (!ex.IsCriticalException()) {
            }
        }

        protected string GetFileServerPath(string path) {
            return $"{_listener.Prefixes.First()}{path}";
        }

        public abstract Task HandleRequestAsync(HttpListenerContext context, CancellationToken ct = default(CancellationToken));
    }
}

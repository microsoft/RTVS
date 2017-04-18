// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using static System.FormattableString;

namespace Microsoft.R.Host.Client.BrokerServices {
    public class WebServer {
        private static ConcurrentDictionary<int, WebServer> Servers { get; } = new ConcurrentDictionary<int, WebServer>();

        private readonly IRemoteUriWebService _remoteUriService;
        private readonly string _baseAddress;
        private readonly IActionLog _log;
        private readonly IConsole _console;
        private readonly string _name;

        private HttpListener _listener;

        public string LocalHost { get; }
        public int LocalPort { get; private set; }
        public string RemoteHost { get; }
        public int RemotePort { get; }

        private WebServer(string remoteHostIp, int remotePort, string baseAddress, string name, IActionLog log, IConsole console) {
            _name = name.ToUpperInvariant();
            _baseAddress = baseAddress;
            _log = log;
            _console = console;

            LocalHost = IPAddress.Loopback.ToString();
            RemoteHost = remoteHostIp;
            RemotePort = remotePort;

            _remoteUriService = new RemoteUriWebService(baseAddress, log, console);
        }

        public async Task InitializeAsync(CancellationToken ct) {
            await TaskUtilities.SwitchToBackgroundThread();
            Random r = new Random();

            // if remote port is between 10000 and 32000, select a port in the same range.
            // R Help uses ports in that range.
            int localPortMin = (RemotePort >= 10000 && RemotePort <= 32000) ? 10000 : 49152;
            int localPortMax = (RemotePort >= 10000 && RemotePort <= 32000) ? 32000 : 65535;

            _console.WriteErrorLine(Resources.Info_RemoteWebServerStarting.FormatInvariant(_name));

            while (true) {
                ct.ThrowIfCancellationRequested();

                _listener = new HttpListener();
                LocalPort = r.Next(localPortMin, localPortMax);
                _listener.Prefixes.Add(Invariant($"http://{LocalHost}:{LocalPort}/"));
                try {
                    _listener.Start();
                } catch (HttpListenerException) {
                    _listener.Close();
                    continue;
                } catch (ObjectDisposedException) {
                    // Socket got closed
                    _log.WriteLine(LogVerbosity.Minimal, MessageCategory.Error, Resources.Error_RemoteWebServerCreationFailed.FormatInvariant(_name));
                    _console.WriteErrorLine(Resources.Error_RemoteWebServerCreationFailed.FormatInvariant(_name));
                    throw new OperationCanceledException();
                }
                break;
            }

            try {
                _log.WriteLine(LogVerbosity.Minimal, MessageCategory.General, Resources.Info_RemoteWebServerStarted.FormatInvariant(_name, LocalHost, LocalPort));
                _console.WriteErrorLine(Resources.Info_RemoteWebServerStarted.FormatInvariant(_name, LocalHost, LocalPort));
                _console.WriteErrorLine(Resources.Info_RemoteWebServerDetails.FormatInvariant(Environment.MachineName, LocalHost, LocalPort, _name, _baseAddress));
            } catch {
            }
        }

        private void Stop() {
            try {
                if (_listener.IsListening) {
                    _listener.Stop();
                }
                _listener.Close();
                _log.WriteLine(LogVerbosity.Minimal, MessageCategory.General, Resources.Info_RemoteWebServerStopped.FormatInvariant(_name));
                _console.WriteErrorLine(Resources.Info_RemoteWebServerStopped.FormatInvariant(_name));
            } catch (Exception ex) when (!ex.IsCriticalException()) {
            }
        }

        public static void Stop(int port) {
            WebServer server;
            if (Servers.TryRemove(port, out server)) {
                server.Stop();
            }
        }

        public static void StopAll() {
            var ports = Servers.Keys.AsArray();
            foreach (var port in ports) {
                Stop(port);
            }
        }

        private async Task DoWorkAsync(CancellationToken ct = default(CancellationToken)) {
            try {
                while (_listener.IsListening) {
                    if (ct.IsCancellationRequested) {
                        _listener.Stop();
                        break;
                    }

                    HttpListenerContext context = await _listener.GetContextAsync();

                    string localUrl = $"{LocalHost}:{LocalPort}";
                    string remoteUrl = $"{RemoteHost}:{RemotePort}";

                    _remoteUriService.GetResponseAsync(context, localUrl, remoteUrl, ct).DoNotWait();
                }
            } catch(Exception ex) {
                if (Servers.ContainsKey(RemotePort)) {
                    // Log only if we expect this web server to be running and it fails.
                    _log.WriteLine(LogVerbosity.Minimal, MessageCategory.Error, Resources.Error_RemoteWebServerFailed.FormatInvariant(_name, ex.Message));
                    _console.WriteErrorLine(Resources.Error_RemoteWebServerFailed.FormatInvariant(_name, ex.Message));
                }
            } finally {
                Stop(RemotePort);
            }
        }

        public static async Task<string> CreateWebServerAsync(string remoteUrl, string baseAddress, string name, IActionLog log, IConsole console, CancellationToken ct = default(CancellationToken)) {
            var remoteUri = new Uri(remoteUrl);
            var localUri = new UriBuilder(remoteUri);

            WebServer server;
            if(!Servers.TryGetValue(remoteUri.Port, out server)) {
                server = new WebServer(remoteUri.Host, remoteUri.Port, baseAddress, name, log, console);
                await server.InitializeAsync(ct);
                Servers.TryAdd(remoteUri.Port, server);
            }

            server.DoWorkAsync(ct).DoNotWait();

            localUri.Host = server.LocalHost;
            localUri.Port = server.LocalPort;
            return localUri.Uri.ToString();
        }
    }
}

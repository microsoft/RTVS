// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;

namespace Microsoft.R.Host.Client.BrokerServices {
    public class WebServer {
        public string LocalHost { get; }
        public int LocalPort { get; }
        public string RemoteHost { get; }
        public int RemotePort { get; }

        private readonly HttpListener _listener;
        private readonly IRemoteUriWebService _remoteUriService;

        private static Dictionary<int, WebServer> Servers { get; } = new Dictionary<int, WebServer>();

        private WebServer(string remoteHostIp, int remotePort,  string baseAddress) {
            LocalHost = IPAddress.Loopback.ToString();
            RemoteHost = remoteHostIp;
            RemotePort = remotePort;

            _remoteUriService = new RemoteUriWebService(baseAddress);
            Random r = new Random();

            // if remote port is between 10000 and 32000, select a port in the same range.
            // R Help uses ports in that range.
            int localPortMin = (RemotePort >= 10000 && RemotePort <= 32000)? 10000: 49152;
            int localPortMax = (RemotePort >= 10000 && RemotePort <= 32000) ? 32000 : 65535;

            while(true) {
                _listener = new HttpListener();
                LocalPort = r.Next(localPortMin, localPortMax);
                _listener.Prefixes.Add($"http://{LocalHost}:{LocalPort}/");
                try {
                    _listener.Start();
                } catch (HttpListenerException) {
                    _listener.Close();
                    continue;
                }
                break;
            }
        }

        private void Stop() {
            if (_listener.IsListening) {
                _listener.Stop();
            }
            _listener.Close();
        }

        public static void Stop(int port) {
            WebServer server;
            if(Servers.TryGetValue(port, out server)) {
                Servers.Remove(port);
                server.Stop();
            }
        }

        private async Task DoWorkAsync(CancellationToken ct) {
            while (_listener.IsListening) {
                if (ct.IsCancellationRequested) {
                    _listener.Stop();
                    break;
                }

                HttpListenerContext context = await _listener.GetContextAsync();

                string localUrl = $"{LocalHost}:{LocalPort}";
                string remoteUrl = $"{RemoteHost}:{RemotePort}";

                await _remoteUriService.GetResponseAsync(context, localUrl, remoteUrl, ct);
            }
        }

        public static string CreateWebServer(string remoteUrl, string baseAddress, CancellationToken ct) {
            Uri remoteUri = new Uri(remoteUrl);
            UriBuilder localUri = new UriBuilder(remoteUri);

            if(!remoteUri.Scheme.EqualsIgnoreCase("http")) {
                // TODO: reject
            }

            WebServer server = null;
            if (Servers.ContainsKey(remoteUri.Port)) {
                server = Servers[remoteUri.Port];
            } else {
                server = new WebServer(remoteUri.Host, remoteUri.Port, baseAddress);
                Servers.Add(remoteUri.Port, server);
            }
            
            server.DoWorkAsync(ct).DoNotWait();

            localUri.Host = server.LocalHost;
            localUri.Port = server.LocalPort;
            return localUri.Uri.ToString();
        }
    }
}

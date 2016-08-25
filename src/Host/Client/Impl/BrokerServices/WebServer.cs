// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using Microsoft.R.Host.Protocol;
using Microsoft.Common.Core;
using System.Net.Sockets;

namespace Microsoft.R.Host.Client.BrokerServices {


    public class WebServer {
        private readonly string _newHost;
        public string Host => _newHost;

        private readonly int _newPort;
        public int Port => _newPort;

        private readonly string _host;
        public string RemoteHost => _host;

        private readonly int _port;
        public int RemotePort => _port;

        private HttpListener _listener;

        private static List<WebServer> _servers = new List<WebServer>();
        public static IReadOnlyList<WebServer> Servers => _servers;

        private readonly RemoteUriWebService _service;
        private IRemoteUriWebService RemoteUriService => _service;

        private WebServer(string remoteHostIp, int remotePort, HttpClient httpClient) {
            _newHost = IPAddress.Loopback.ToString();
            _host = remoteHostIp;
            _port = remotePort;
            _service = new RemoteUriWebService(httpClient);
            Random r = new Random();
            while(true) {
                _listener = new HttpListener();
                _newPort = r.Next(49152, 65535);
                _listener.Prefixes.Add($"http://{_newHost}:{_newPort}/");

                try {
                    _listener.Start();
                } catch (HttpListenerException ex) {
                    continue;
                }
                break;
            }
        }

        private void Stop() {
            if (_listener.IsListening) {
                _listener.Stop();
            }
        }

        private async Task DoWorkAsync(CancellationToken ct) {
            while (_listener.IsListening) {
                if (ct.IsCancellationRequested) {
                    _listener.Stop();
                    break;
                }

                HttpListenerContext context = await _listener.GetContextAsync();
                var response = await _service.PostAsync(RemoteUriRequest.Create(context.Request, RemoteHost, RemotePort));

                var webHeaders = new WebHeaderCollection();
                webHeaders.Add(response.Headers);
                context.Response.Headers = webHeaders;
                StreamWriter writer = new StreamWriter(context.Response.OutputStream);
                await writer.WriteAsync(response.Content);
            }
        }
        
        public static string CreateWebServer(string remoteUrl, HttpClient httpClient, CancellationToken ct) {
            Uri remoteUri = new Uri(remoteUrl);
            UriBuilder localUri = new UriBuilder(remoteUri);
            var server = new WebServer(remoteUri.Host, remoteUri.Port, httpClient);
            _servers.Add(server);

            server.DoWorkAsync(ct).DoNotWait();

            localUri.Host = server.Host;
            localUri.Port = server.Port;
            return localUri.Uri.ToString();
        }

        private static int GetAvaialblePort() {
            throw new NotImplementedException();
        }
    }
}

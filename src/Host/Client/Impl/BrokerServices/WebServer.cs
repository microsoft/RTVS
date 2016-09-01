// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Newtonsoft.Json;

namespace Microsoft.R.Host.Client.BrokerServices {
    public class WebServer {
        private readonly string _newHost;
        public string Host => _newHost;

        private readonly int _newPort;
        public int Port => _newPort;

        private readonly string _remoteHost;
        public string RemoteHost => _remoteHost;

        private readonly int _remotePort;
        public int RemotePort => _remotePort;

        private HttpListener _listener;

        private static List<WebServer> _servers = new List<WebServer>();
        public static IReadOnlyList<WebServer> Servers => _servers;

        private readonly RemoteUriWebService _service;
        private IRemoteUriWebService RemoteUriService => _service;

        private WebServer(string remoteHostIp, int remotePort, HttpClient httpClient) {
            _newHost = IPAddress.Loopback.ToString();
            _remoteHost = remoteHostIp;
            _remotePort = remotePort;
            _service = new RemoteUriWebService(httpClient);
            Random r = new Random();

            // if remote port is between 10000 and 32000, select a port in the same range.
            // R Help uses ports in that range.
            int localPortMin = (_remotePort >= 10000 && _remotePort <= 32000)? 10000: 49152;
            int localPortMax = (_remotePort >= 10000 && _remotePort <= 32000) ? 32000 : 65535;

            while(true) {
                _listener = new HttpListener();
                _newPort = r.Next(localPortMin, localPortMax);
                _listener.Prefixes.Add($"http://{_newHost}:{_newPort}/");

                try {
                    _listener.Start();
                } catch (HttpListenerException) {
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

                RemoteUriResponse resp = null;
                using (var requestStream = await CreateRequestAsync(context.Request, RemoteHost, RemotePort)) {
                    var responseStream = await _service.PostAsync(requestStream);
                    resp = await ParseResponseAsync(responseStream);
                }

                string localHostPort = $"{_newHost}:{_newPort}";
                string remoteHostPort = $"{_remoteHost}:{_remotePort}";

                var webHeaders = new WebHeaderCollection();
                foreach (var pair in resp.Headers) {
                    string value = pair.Value;
                    value = value.Replace(remoteHostPort, localHostPort);
                    webHeaders.Add($"{pair.Key}:{value}");
                }

                context.Response.Headers = webHeaders;
                await resp.Content.CopyToAsync(context.Response.OutputStream);
                await context.Response.OutputStream.FlushAsync();
                context.Response.OutputStream.Close();
            }
        }

        private async Task<Stream> CreateRequestAsync(HttpListenerRequest request, string remoteHost, int remotePort) {
            UriBuilder ub = new UriBuilder(request.Url);
            string localHostPort = $"{ub.Host}:{ub.Port}";
            string remoteHostPort = $"{remoteHost}:{remotePort}";
            ub.Host = remoteHost;
            ub.Port = remotePort;

            var headers = new Dictionary<string, string>();
            foreach (string key in request.Headers.AllKeys) {
                string value = request.Headers[key];
                value = value.Replace(localHostPort, remoteHostPort);
                headers.Add(key, value);
            }

            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            writer.Write(ub.Uri.ToString());
            writer.Write(request.HttpMethod);
            writer.Write(JsonConvert.SerializeObject(headers));
            await request.InputStream.CopyToAsync(ms);
            writer.Flush();
            await ms.FlushAsync();
            ms.Position = 0;
            return ms;
        }

        private async Task<RemoteUriResponse> ParseResponseAsync(Stream response) {
            RemoteUriResponse resp = new RemoteUriResponse();
            
            BinaryReader reader = new BinaryReader(response);
            string headerString = reader.ReadString();
            resp.Headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(headerString);

            long length = response.Length - response.Position;
            byte[] buffer = new byte[length];
            await response.ReadAsync(buffer, 0, buffer.Length);

            resp.Content = new MemoryStream(buffer);
            resp.Content.Position = 0;
            return resp;
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

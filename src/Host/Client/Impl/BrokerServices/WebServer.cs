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
using System.Text;

namespace Microsoft.R.Host.Client.BrokerServices {
    public class WebServer {
        public string Host { get; }
        public int Port { get; }
        public string RemoteHost { get; }
        public int RemotePort { get; }

        private HttpListener _listener;

        private static Dictionary<int, WebServer> _servers = new Dictionary<int, WebServer>();
        public static IReadOnlyDictionary<int, WebServer> Servers => _servers;

        private IRemoteUriWebService RemoteUriService { get; }

        private class RemoteUriResponse {
            public string ContentType { get; set; }
            public long ContentLength { get; set; }
            public int StatusCode { get; set; }
            public Dictionary<string, string> Headers { get; set; }
            public Stream Content { get; set; }
        }

        private WebServer(string remoteHostIp, int remotePort, HttpClient httpClient) {
            Host = IPAddress.Loopback.ToString();
            RemoteHost = remoteHostIp;
            RemotePort = remotePort;
            RemoteUriService = new RemoteUriWebService(httpClient);
            Random r = new Random();

            // if remote port is between 10000 and 32000, select a port in the same range.
            // R Help uses ports in that range.
            int localPortMin = (RemotePort >= 10000 && RemotePort <= 32000)? 10000: 49152;
            int localPortMax = (RemotePort >= 10000 && RemotePort <= 32000) ? 32000 : 65535;

            while(true) {
                _listener = new HttpListener();
                Port = r.Next(localPortMin, localPortMax);
                _listener.Prefixes.Add($"http://{Host}:{Port}/");

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
            if (_listener.IsListening) {
                // There is already a thread listening.
                return;
            }

            while (_listener.IsListening) {
                if (ct.IsCancellationRequested) {
                    _listener.Stop();
                    break;
                }

                HttpListenerContext context = await _listener.GetContextAsync();

                RemoteUriResponse resp = null;
                using (var requestStream = await CreateRequestAsync(context.Request, RemoteHost, RemotePort)) {
                    var responseStream = await RemoteUriService.PostAsync(requestStream);
                    resp = await ParseResponseAsync(responseStream);
                }

                string localHostPort = $"{Host}:{Port}";
                string remoteHostPort = $"{RemoteHost}:{RemotePort}";

                foreach (var pair in resp.Headers) {
                    string value = pair.Value;
                    value = value.Replace(remoteHostPort, localHostPort);
                    context.Response.AddHeader(pair.Key, value);
                }

                context.Response.ContentLength64 = resp.ContentLength;

                if (!string.IsNullOrWhiteSpace(resp.ContentType)) {
                    using (MemoryStream ms = new MemoryStream()) {
                        // using the internal media type parser by using a temp response message
                        HttpResponseMessage tempMessage = new HttpResponseMessage();
                        tempMessage.Content = new StreamContent(ms);
                        tempMessage.Content.Headers.Add("Content-Type", resp.ContentType);
                        context.Response.ContentType = tempMessage.Content.Headers.ContentType.MediaType;
                        if (!string.IsNullOrWhiteSpace(tempMessage.Content.Headers.ContentType.CharSet)) {
                            context.Response.ContentEncoding = Encoding.GetEncoding(tempMessage.Content.Headers.ContentType.CharSet);
                        }
                    }
                }

                context.Response.StatusCode = resp.StatusCode;
                if(resp.Content.Length != 0) {
                    await resp.Content.CopyToAsync(context.Response.OutputStream);
                    await context.Response.OutputStream.FlushAsync();
                }
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
            resp.StatusCode = reader.ReadInt32();
            string headerString = reader.ReadString();
            resp.Headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(headerString);

            long length = response.Length - response.Position;
            byte[] buffer = new byte[length];
            await response.ReadAsync(buffer, 0, buffer.Length);

            resp.Content = new MemoryStream(buffer);
            resp.Content.Position = 0;

            string contentTypeKey = "Content-Type";
            if (resp.Headers.ContainsKey(contentTypeKey)) {
                resp.ContentType = resp.Headers[contentTypeKey];
                resp.Headers.Remove(contentTypeKey);
            }

            string contentLengthKey = "Content-Length";
            if (resp.Headers.ContainsKey(contentLengthKey)) {
                long result = 0;
                if (!long.TryParse(resp.Headers[contentLengthKey], out result) || result < 0) {
                    // use the calculated length
                    result = length;
                }
                resp.ContentLength = result;
                resp.Headers.Remove(contentLengthKey);
            }

            return resp;
        }

        public static string CreateWebServer(string remoteUrl, HttpClient httpClient, CancellationToken ct) {
            Uri remoteUri = new Uri(remoteUrl);
            UriBuilder localUri = new UriBuilder(remoteUri);

            WebServer server = null;
            if (!_servers.ContainsKey(remoteUri.Port)) {
                server = _servers[remoteUri.Port];
            } else {
                server = new WebServer(remoteUri.Host, remoteUri.Port, httpClient);
                _servers.Add(remoteUri.Port, server);
            }
            
            server.DoWorkAsync(ct).DoNotWait();

            localUri.Host = server.Host;
            localUri.Port = server.Port;
            return localUri.Uri.ToString();
        }
    }
}

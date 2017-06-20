// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebSockets.Protocol;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using Microsoft.R.Host.Protocol;
using static System.FormattableString;

namespace Microsoft.R.Host.Client.BrokerServices {
    internal class RemoteUriWebService : IRemoteUriWebService {
        private readonly IActionLog _log;
        private readonly IConsole _console;
        private const int _receiveBufferSize = 0x1000; // buffer size for ClientWebSocket

        public RemoteUriWebService(string baseUri, IActionLog log, IConsole console) {
            PostUri = new Uri(new Uri(baseUri), new Uri("/remoteuri", UriKind.Relative));
            _log = log;
            _console = console;
         }

        private Uri PostUri { get; }

        public async Task GetResponseAsync(HttpListenerContext context, string localBaseUrl, string remoteBaseUrl, CancellationToken ct) {
            string postUri = null;

            if (context.Request.IsWebSocketRequest) {
                var ub = new UriBuilder(PostUri) { Scheme = "wss" };
                postUri = ub.Uri.ToString();
            } else {
                postUri = PostUri.ToString();
            }

            var request = (HttpWebRequest)WebRequest.Create(postUri);
            request.Method = context.Request.HttpMethod;
            request.ServerCertificateValidationCallback += ValidateCertificate;

            if (!context.Request.IsWebSocketRequest) {
                SetRequestHeaders(request, context.Request.Headers, localBaseUrl, remoteBaseUrl);
            }

            // Add RTVS headers
            var remoteUri = GetRemoteUri(context.Request.Url, remoteBaseUrl);
            request.Headers.Add(CustomHttpHeaders.RTVSRequestedURL, remoteUri.ToString());

            if (context.Request.ContentLength64 > 0) {
                using (var reqStream = await request.GetRequestStreamAsync()) {
                    await context.Request.InputStream.CopyAndFlushAsync(reqStream, null, ct);
                }
            }

            HttpWebResponse response = null;
            try {
                response = (HttpWebResponse)await request.GetResponseAsync();
                if (response != null) {
                    if (context.Request.IsWebSocketRequest && response.StatusCode == HttpStatusCode.SwitchingProtocols) {
                        var respStream = response.GetResponseStream();
                        var subProtocol = response.Headers[Constants.Headers.SecWebSocketProtocol];
                        var remoteWebSocket = CommonWebSocket.CreateClientWebSocket(respStream, subProtocol, TimeSpan.FromMinutes(10), receiveBufferSize: _receiveBufferSize, useZeroMask: true);
                        var websocketContext = await context.AcceptWebSocketAsync(subProtocol, receiveBufferSize: _receiveBufferSize, keepAliveInterval: TimeSpan.FromMinutes(10));
                        await WebSocketHelper.SendReceiveAsync(websocketContext.WebSocket, remoteWebSocket, ct);
                    } else {
                        context.Response.StatusCode = (int)response.StatusCode;
                        SetResponseHeaders(response, context.Response, localBaseUrl, remoteBaseUrl);
                        using (var respStream = response.GetResponseStream())
                        using (var outStream = context.Response.OutputStream) {
                            await respStream.CopyAndFlushAsync(outStream, null, ct);
                        }
                        response.Close();
                    }
                }
            } catch (WebException wex) when (wex.Status == WebExceptionStatus.ProtocolError) {
                response = wex.Response as HttpWebResponse;
            } catch (OperationCanceledException) {
                WebServer.Stop(remoteUri.Port);
            } catch (Exception ex) when (!ex.IsCriticalException()) {
                _log.WriteLine(LogVerbosity.Normal, MessageCategory.Error, Resources.Error_RemoteWebServerException.FormatInvariant(ex.Message));
                _console?.WriteErrorLine(Resources.Error_RemoteWebServerException.FormatInvariant(ex.Message));
                WebServer.Stop(remoteUri.Port);
            } finally {
                response?.Close();
            }
        }

        private Uri GetRemoteUri(Uri url, string remoteBase) {
            var remote = new Uri(Invariant($"http://{remoteBase}"));
            var ub = new UriBuilder(url) {
                Host = remote.Host,
                Port = remote.Port
            };
            return ub.Uri;
        }

        private static string ReplaceAndGet(string value, string url1, string url2) => value.Replace(url1, url2);

        private static void SetRequestHeaders(HttpWebRequest request, NameValueCollection requestHeaders, string localBaseUrl, string remoteBaseUrl) {
            // copy headers to avoid messing with original request headers
            var headers = new Dictionary<string, string>();
            foreach (var key in requestHeaders.AllKeys) {
                headers.Add(key, requestHeaders[key]);
            }

            if (headers.TryGetValue("Accept", out var valueAccept)) {
                request.Accept = ReplaceAndGet(valueAccept, localBaseUrl, remoteBaseUrl);
                headers.Remove("Accept");
            }

            if (headers.TryGetValue("Connection", out var valueConnection)) {
                if (valueConnection.EqualsIgnoreCase("keep-alive")) {
                    request.KeepAlive = true;
                } else if (valueConnection.EqualsIgnoreCase("close")) {
                    request.KeepAlive = false;
                } else {
                    request.Connection = valueConnection;
                }
                headers.Remove("Connection");
            }

            string valueContentLength;
            if (headers.TryGetValue("Content-Length", out valueContentLength)) {
                request.ContentLength = valueContentLength.ToLongOrDefault();
                headers.Remove("Content-Length");
            }

            if (headers.TryGetValue("Content-Type", out var valueContentType)) {
                request.ContentType = valueContentType;
                headers.Remove("Content-Type");
            }

            if (headers.TryGetValue("Expect", out var valueExpect)) {
                request.Expect = valueExpect;
                headers.Remove("Expect");
            }

            if (headers.TryGetValue("Date", out var valueDate)) {
                request.Date = valueDate.ToDateTimeOrDefault();
                headers.Remove("Date");
            }

            if (headers.TryGetValue("Host", out var valueHost)) {
                request.Host = ReplaceAndGet(valueHost, localBaseUrl, remoteBaseUrl);
                headers.Remove("Host");
            }

            if (headers.TryGetValue("If-Modified-Since", out var valueIfModifiedSince)) {
                request.IfModifiedSince = valueIfModifiedSince.ToDateTimeOrDefault();
                headers.Remove("If-Modified-Since");
            }

            if (headers.TryGetValue("Range", out var valueRange)) {
                // TODO: AddRange
                headers.Remove("Range");
            }

            if (headers.TryGetValue("Referer", out var valueReferer)) {
                request.Referer = ReplaceAndGet(valueReferer, localBaseUrl, remoteBaseUrl);
                headers.Remove("Referer");
            }

            if (headers.TryGetValue("Transfer-Encoding", out var valueTransferEncoding)) {
                request.SendChunked = true;
                request.TransferEncoding = valueTransferEncoding;
                headers.Remove("Transfer-Encoding");
            }

            if (headers.TryGetValue("User-Agent", out var valueUserAgent)) {
                request.UserAgent = valueUserAgent;
                headers.Remove("User-Agent");
            }

            foreach (var pair in headers) {
                request.Headers.Add(pair.Key, ReplaceAndGet(pair.Value, localBaseUrl, remoteBaseUrl));
            }
        }

        private static void SetResponseHeaders(HttpWebResponse response, HttpListenerResponse httpListenerResponse, string localBaseUrl, string remoteBaseUrl) {
            // copy headers to avoid messing with original response headers
            var headers = new Dictionary<string, string>();
            foreach (var key in response.Headers.AllKeys) {
                headers.Add(key, ReplaceAndGet(response.Headers[key], remoteBaseUrl, localBaseUrl));
            }

            if(response.ContentLength >= 0) {
                httpListenerResponse.ContentLength64 = response.ContentLength;
            }
            httpListenerResponse.ContentType = response.ContentType;

            if(headers.TryGetValue("Transfer-Encoding", out string valueTransferEncoding)) {
                httpListenerResponse.SendChunked = valueTransferEncoding.EqualsIgnoreCase("chunked");
                headers.Remove("Transfer-Encoding");
            }

            if (!string.IsNullOrWhiteSpace(response.ContentEncoding)) {
                httpListenerResponse.ContentEncoding = Encoding.GetEncoding(response.ContentEncoding);
            }

            httpListenerResponse.StatusCode = (int)response.StatusCode;

            if (headers.ContainsKey("Content-Length")) {
                headers.Remove("Content-Length");
            }

            if (headers.ContainsKey("Content-Type")) {
                headers.Remove("Content-Type");
            }

            if (headers.ContainsKey("Content-Encoding")) {
                headers.Remove("Content-Encoding");
            }

            foreach (var pair in headers) {
                httpListenerResponse.Headers.Add(pair.Key, pair.Value);
            }
        }

        private bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
            if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) != 0) {
                return false;
            }
            // Accept other cases. Main certificate validation is done at the time we connect to the broker.
            return true;
        }
    }
}

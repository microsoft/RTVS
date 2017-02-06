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
using Microsoft.Common.Core.Services;
using Microsoft.R.Host.Protocol;
using static System.FormattableString;

namespace Microsoft.R.Host.Client.BrokerServices {
    public class RemoteUriWebService : IRemoteUriWebService {
        private ICoreServices _services;
        private IConsole _console;

        private IActionLog Log => _services?.Log;

        public RemoteUriWebService(string baseUri, ICoreServices services, IConsole console) {
            PostUri = new Uri(new Uri(baseUri), new Uri("/remoteuri", UriKind.Relative));
            _services = services;
            _console = console;
        }

        private Uri PostUri { get; }

        public async Task GetResponseAsync(HttpListenerContext context, string localBaseUrl, string remoteBaseUrl, CancellationToken ct) {
            string postUri = null;

            if (context.Request.IsWebSocketRequest) {
                UriBuilder ub = new UriBuilder(PostUri) { Scheme = "wss" };
                postUri = ub.Uri.ToString();
            } else {
                postUri = PostUri.ToString();
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(postUri);
            request.Method = context.Request.HttpMethod;
            request.ServerCertificateValidationCallback += ValidateCertificate;

            if (!context.Request.IsWebSocketRequest) {
                SetRequestHeaders(request, context.Request.Headers, localBaseUrl, remoteBaseUrl);
            }

            // Add RTVS headers
            var remoteUri = GetRemoteUri(context.Request.Url, remoteBaseUrl);
            request.Headers.Add(CustomHttpHeaders.RTVSRequestedURL, remoteUri.ToString());

            if (context.Request.InputStream.CanSeek && context.Request.InputStream.Length > 0) {
                using (Stream reqStream = await request.GetRequestStreamAsync()) {
                    await context.Request.InputStream.CopyAndFlushAsync(reqStream, null, ct);
                }
            }

            HttpWebResponse response = null;
            try {
                response = (HttpWebResponse)await request.GetResponseAsync();
                if (response != null) {
                    if (context.Request.IsWebSocketRequest && response.StatusCode == HttpStatusCode.SwitchingProtocols) {
                        Stream respStream = response.GetResponseStream();
                        string subProtocol = response.Headers[Constants.Headers.SecWebSocketProtocol];
                        var remoteWebSocket = CommonWebSocket.CreateClientWebSocket(respStream, subProtocol, TimeSpan.FromMinutes(10), receiveBufferSize: 65335, useZeroMask: true);
                        var websocketContext = await context.AcceptWebSocketAsync(subProtocol, receiveBufferSize: 65335, keepAliveInterval: TimeSpan.FromMinutes(10));
                        await WebSocketHelper.SendReceiveAsync(websocketContext.WebSocket, remoteWebSocket, ct);
                    } else {
                        context.Response.StatusCode = (int)response.StatusCode;
                        SetResponseHeaders(response, context.Response, localBaseUrl, remoteBaseUrl);
                        using (Stream respStream = response.GetResponseStream())
                        using (Stream outStream = context.Response.OutputStream) {
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
                Log?.WriteLine(LogVerbosity.Normal, MessageCategory.Error, Resources.Error_RemoteWebServerException.FormatInvariant(ex.Message));
                _console?.WriteErrorLine(Resources.Error_RemoteWebServerException.FormatInvariant(ex.Message));
                WebServer.Stop(remoteUri.Port);
            } finally {
                response?.Close();
            }
        }

        private Uri GetRemoteUri(Uri url, string remoteBase) {
            Uri remote = new Uri(Invariant($"http://{remoteBase}"));
            UriBuilder ub = new UriBuilder(url);
            ub.Host = remote.Host;
            ub.Port = remote.Port;
            return ub.Uri;
        }

        private static string ReplaceAndGet(string value, string url1, string url2) {
            return value.Replace(url1, url2);
        }

        private static void SetRequestHeaders(HttpWebRequest request, NameValueCollection requestHeaders, string localBaseUrl, string remoteBaseUrl) {
            // copy headers to avoid messing with original request headers
            Dictionary<string, string> headers = new Dictionary<string, string>();
            foreach (string key in requestHeaders.AllKeys) {
                headers.Add(key, requestHeaders[key]);
            }

            string valueAccept;
            if (headers.TryGetValue("Accept", out valueAccept)) {
                request.Accept = ReplaceAndGet(valueAccept, localBaseUrl, remoteBaseUrl);
                headers.Remove("Accept");
            }

            string valueConnection;
            if (headers.TryGetValue("Connection", out valueConnection)) {
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

            string valueContentType;
            if (headers.TryGetValue("Content-Type", out valueContentType)) {
                request.ContentType = valueContentType;
                headers.Remove("Content-Type");
            }

            string valueExpect;
            if (headers.TryGetValue("Expect", out valueExpect)) {
                request.Expect = valueExpect;
                headers.Remove("Expect");
            }

            string valueDate;
            if (headers.TryGetValue("Date", out valueDate)) {
                request.Date = valueDate.ToDateTimeOrDefault();
                headers.Remove("Date");
            }

            string valueHost;
            if (headers.TryGetValue("Host", out valueHost)) {
                request.Host = ReplaceAndGet(valueHost, localBaseUrl, remoteBaseUrl);
                headers.Remove("Host");
            }

            string valueIfModifiedSince;
            if (headers.TryGetValue("If-Modified-Since", out valueIfModifiedSince)) {
                request.IfModifiedSince = valueIfModifiedSince.ToDateTimeOrDefault();
                headers.Remove("If-Modified-Since");
            }

            string valueRange;
            if (headers.TryGetValue("Range", out valueRange)) {
                // TODO: AddRange
                headers.Remove("Range");
            }

            string valueReferer;
            if (headers.TryGetValue("Referer", out valueReferer)) {
                request.Referer = ReplaceAndGet(valueReferer, localBaseUrl, remoteBaseUrl);
                headers.Remove("Referer");
            }

            string valueTransferEncoding;
            if (headers.TryGetValue("Transfer-Encoding", out valueTransferEncoding)) {
                request.SendChunked = true;
                request.TransferEncoding = valueTransferEncoding;
                headers.Remove("Transfer-Encoding");
            }

            string valueUserAgent;
            if (headers.TryGetValue("User-Agent", out valueUserAgent)) {
                request.UserAgent = valueUserAgent;
                headers.Remove("User-Agent");
            }

            foreach (var pair in headers) {
                request.Headers.Add(pair.Key, ReplaceAndGet(pair.Value, localBaseUrl, remoteBaseUrl));
            }
        }

        private static void SetResponseHeaders(HttpWebResponse response, HttpListenerResponse httpListenerResponse, string localBaseUrl, string remoteBaseUrl) {
            // copy headers to avoid messing with original response headers
            Dictionary<string, string> headers = new Dictionary<string, string>();
            foreach (var key in response.Headers.AllKeys) {
                headers.Add(key, ReplaceAndGet(response.Headers[key], remoteBaseUrl, localBaseUrl));
            }

            httpListenerResponse.ContentLength64 = response.ContentLength;
            httpListenerResponse.ContentType = response.ContentType;

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

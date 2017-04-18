// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebSockets.Protocol;
using System.Net.Security;
using Microsoft.R.Host.Client;

namespace Microsoft.AspNetCore.WebSockets.Client {
    public class WebSocketClient : IWebSocketClient {
        static WebSocketClient() {
            try {
                // Only call once
                WebSocket.RegisterPrefixes();
            } catch (Exception) {
                // Already registered
            }
        }

        public WebSocketClient(IList<string> subProtocols) {
            ReceiveBufferSize = 1024 * 16;
            KeepAliveInterval = TimeSpan.FromMinutes(2);
            SubProtocols = subProtocols;
        }

        public IList<string> SubProtocols { get; }
        public TimeSpan KeepAliveInterval { get; set; }
        public int ReceiveBufferSize { get; set; }
        public bool UseZeroMask { get; set; }
        public Action<HttpWebResponse> InspectResponse { get; set; }

        public HttpWebRequest CreateRequest(Uri uri, ICredentials serverCredentails = null) {
            var request = (HttpWebRequest)WebRequest.Create(uri);

            request.Headers[Constants.Headers.SecWebSocketVersion] = Constants.Headers.SupportedVersion;
            if (SubProtocols.Count > 0) {
                request.Headers[Constants.Headers.SecWebSocketProtocol] = string.Join(", ", SubProtocols);
            }
            request.AuthenticationLevel = AuthenticationLevel.MutualAuthRequested;
            if (serverCredentails != null) {
                request.Credentials = serverCredentails;
            }
            return request;
        }

        public async Task<WebSocket> ConnectAsync(HttpWebRequest request, CancellationToken cancellationToken) {
            HttpWebResponse response;

            using (cancellationToken.Register(request.Abort)) {
                response = (HttpWebResponse)await request.GetResponseAsync();
            }

            InspectResponse?.Invoke(response);

            // TODO: Validate handshake
            HttpStatusCode statusCode = response.StatusCode;
            if (statusCode != HttpStatusCode.SwitchingProtocols) {
                response.Dispose();
                throw new InvalidOperationException("Incomplete handshake, invalid status code: " + statusCode);
            }
            // TODO: Validate Sec-WebSocket-Key/Sec-WebSocket-Accept

            string subProtocol = response.Headers[Constants.Headers.SecWebSocketProtocol];
            if (!string.IsNullOrEmpty(subProtocol) && !SubProtocols.Contains(subProtocol, StringComparer.OrdinalIgnoreCase)) {
                throw new InvalidOperationException("Incomplete handshake, the server specified an unknown sub-protocol: " + subProtocol);
            }

            var stream = response.GetResponseStream();

            return CommonWebSocket.CreateClientWebSocket(stream, subProtocol, KeepAliveInterval, ReceiveBufferSize, UseZeroMask);
        }
    }
}
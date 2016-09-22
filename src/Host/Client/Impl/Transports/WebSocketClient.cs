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

namespace Microsoft.AspNetCore.WebSockets.Client {
    public class WebSocketClient {
        static WebSocketClient() {
            try {
                // Only call once
                WebSocket.RegisterPrefixes();
            } catch (Exception) {
                // Already registered
            }
        }

        public WebSocketClient() {
            ReceiveBufferSize = 1024 * 16;
            KeepAliveInterval = TimeSpan.FromMinutes(2);
            SubProtocols = new List<string>();
        }

        public IList<string> SubProtocols { get; }
        public TimeSpan KeepAliveInterval { get; set; }
        public int ReceiveBufferSize { get; set; }
        public bool UseZeroMask { get; set; }
        public Action<HttpWebRequest> ConfigureRequest { get; set; }
        public Action<HttpWebResponse> InspectResponse { get; set; }

        public async Task<WebSocket> ConnectAsync(Uri uri, CancellationToken cancellationToken) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

            var cancellation = cancellationToken.Register(() => request.Abort());

            request.Headers[Constants.Headers.SecWebSocketVersion] = Constants.Headers.SupportedVersion;
            if (SubProtocols.Count > 0) {
                request.Headers[Constants.Headers.SecWebSocketProtocol] = string.Join(", ", SubProtocols);
            }

            ConfigureRequest?.Invoke(request);

            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();

            cancellation.Dispose();

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
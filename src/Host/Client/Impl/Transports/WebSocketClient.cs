// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client.Transports {
    internal sealed class WebSocketClient {
        private readonly ICredentials _serverCredentails;
        private readonly IEnumerable<string> _subProtocols;
        private readonly TimeSpan _keepAliveInterval;
        private readonly Uri _uri;

        public WebSocketClient(Uri uri, IEnumerable<string> subProtocols, TimeSpan keepAliveInterval, ICredentials serverCredentails = null) {
            _uri = uri;
            _subProtocols = subProtocols;
            _keepAliveInterval = keepAliveInterval;
            _serverCredentails = serverCredentails;
        }

        public async Task<WebSocket> ConnectAsync(CancellationToken cancellationToken) {
            var clientWebsocket = new ClientWebSocket();

            clientWebsocket.Options.Credentials = _serverCredentails;
            clientWebsocket.Options.KeepAliveInterval = _keepAliveInterval;

            foreach (var sb in _subProtocols) {
                clientWebsocket.Options.AddSubProtocol(sb);
            }

            await clientWebsocket.ConnectAsync(_uri, cancellationToken);
            return clientWebsocket;
        }
    }
}
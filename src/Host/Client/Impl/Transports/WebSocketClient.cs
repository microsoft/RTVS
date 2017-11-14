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
            var socket = new ClientWebSocket();

            socket.Options.Credentials = _serverCredentails;
            socket.Options.KeepAliveInterval = _keepAliveInterval;
            socket.Options.SetRequestHeader(Constants.Headers.SecWebSocketVersion, Constants.Headers.SupportedVersion);

            if (_subProtocols.Any()) {
                socket.Options.SetRequestHeader(Constants.Headers.SecWebSocketProtocol, string.Join(", ", _subProtocols));
            }

            foreach (var sb in _subProtocols) {
                socket.Options.AddSubProtocol(sb);
            }

            
            await socket.ConnectAsync(_uri, CancellationToken.None);
            return socket;
        }
    }
}
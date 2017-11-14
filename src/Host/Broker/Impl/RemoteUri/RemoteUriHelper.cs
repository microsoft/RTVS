// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Common.Core;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Broker.RemoteUri {
    public class RemoteUriHelper {
        public static async Task HandlerAsync(HttpContext context) {
            var url = context.Request.Headers[CustomHttpHeaders.RTVSRequestedURL];

            using (var client = new HttpClient()) {
                if (context.WebSockets.IsWebSocketRequest) {
                    var ub = new UriBuilder(url) { Scheme = "ws" };
                    var clientWebsocket = new ClientWebSocket();

                    foreach(var subProtocol in context.WebSockets.WebSocketRequestedProtocols) {
                        clientWebsocket.Options.AddSubProtocol(subProtocol);
                    }

                    clientWebsocket.Options.KeepAliveInterval = TimeSpan.FromMinutes(10);
                    await clientWebsocket.ConnectAsync(ub.Uri, CancellationToken.None);

                    var serverWebSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await WebSocketHelper.SendReceiveAsync(serverWebSocket, clientWebsocket, CancellationToken.None);
                } else {
                    var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), url);
                    foreach (var requestHeader in context.Request.Headers) {
                        IEnumerable<string> value = requestHeader.Value;
                        request.Headers.Add(requestHeader.Key, value);
                    }

                    if (context.Request.ContentLength > 0) {
                        using (var reqStream = await request.Content.ReadAsStreamAsync()) {
                            await context.Request.Body.CopyToAsync(reqStream);
                            await reqStream.FlushAsync();
                        }
                    }

                    using (var response = await client.SendAsync(request)) {
                        context.Response.StatusCode = (int)response.StatusCode;

                        foreach (var responseHeader in context.Response.Headers) {
                            IEnumerable<string> value = responseHeader.Value;
                            response.Headers.Add(responseHeader.Key, value);
                        }

                        using (var respStream = await response.Content.ReadAsStreamAsync()) {
                            await respStream.CopyToAsync(context.Response.Body);
                            await context.Response.Body.FlushAsync();
                        }
                    }
                }
            }
        }
    }
}

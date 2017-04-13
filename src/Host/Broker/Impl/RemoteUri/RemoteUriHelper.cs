// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Common.Core;
using Microsoft.R.Host.Protocol;
using Microsoft.AspNetCore.WebSockets.Protocol;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace Microsoft.R.Host.Broker.RemoteUri {
    public class RemoteUriHelper {
        public static async Task HandlerAsync(HttpContext context) {
            var url = context.Request.Headers[CustomHttpHeaders.RTVSRequestedURL];

            using (var client = new HttpClient()) {
                if (context.WebSockets.IsWebSocketRequest) {
                    var ub = new UriBuilder(url) { Scheme = "ws" };
                    var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), ub.Uri);
                    string subProtocols = string.Join(", ", context.WebSockets.WebSocketRequestedProtocols);

                    request.Headers.Add(Constants.Headers.SecWebSocketVersion, Constants.Headers.SupportedVersion);
                    if (!string.IsNullOrWhiteSpace(subProtocols)) {
                        request.Headers.Add(Constants.Headers.SecWebSocketProtocol, subProtocols);
                    }

                    var response = await client.SendAsync(request);
                    HttpStatusCode statusCode = response.StatusCode;
                    if (statusCode != HttpStatusCode.SwitchingProtocols) {
                        response.Dispose();
                    } else {
                        string respSubProtocol = response.Headers.GetValues(Constants.Headers.SecWebSocketProtocol).FirstOrDefault();
                        // TODO: match sub protocols.

                        var responseStream = await response.Content.ReadAsStreamAsync();
                        var clientWebsocket = CommonWebSocket.CreateClientWebSocket(responseStream, respSubProtocol, TimeSpan.FromMinutes(2), receiveBufferSize: 1024 * 16, useZeroMask: false);
                        var serverWebSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await WebSocketHelper.SendReceiveAsync(serverWebSocket, clientWebsocket, CancellationToken.None);
                    }
                } else {
                    var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), url);
                    foreach (var requestHeader in context.Request.Headers) {
                        IEnumerable<string> value = requestHeader.Value;
                        request.Headers.Add(requestHeader.Key, value);
                    }

                    if (context.Request.ContentLength > 0) {
                        using (Stream reqStream = await request.Content.ReadAsStreamAsync()) {
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

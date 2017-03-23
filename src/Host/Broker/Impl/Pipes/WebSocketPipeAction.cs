// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.R.Host.Broker.Pipes {
    public class WebSocketPipeAction : IActionResult {
        private readonly string _id;
        private readonly IMessagePipeEnd _pipe;

        public WebSocketPipeAction(string id, IMessagePipeEnd pipe) {
            _id = id;
            _pipe = pipe;
        }

        public async Task ExecuteResultAsync(ActionContext actionContext) {
            using (_pipe) {
                var context = actionContext.HttpContext;
                var httpResponse = context.Features.Get<IHttpResponseFeature>();

                if (!context.WebSockets.IsWebSocketRequest) {
                    httpResponse.ReasonPhrase = "Websocket connection expected";
                    httpResponse.StatusCode = 401;
                    return;
                }

                using (var socket = await context.WebSockets.AcceptWebSocketAsync("Microsoft.R.Host")) {
                    Task wsToPipe, pipeToWs, completed;

                    var cts = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
                    wsToPipe = WebSocketToPipeWorker(socket, _pipe, cts.Token);
                    pipeToWs = PipeToWebSocketWorker(socket, _pipe, cts.Token);
                    completed = await Task.WhenAny(wsToPipe, pipeToWs);

                    if (completed == pipeToWs) {
                        // If the pipe end is exhausted, tell the client that there's no more messages to follow,
                        // so that it can gracefully disconnect from its end. 
                        await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", context.RequestAborted);
                    } else {
                        // If the client disconnected, then just cancel any outstanding reads from the pipe.
                        cts.Cancel();
                    }
                }
            }
        }

        private static async Task WebSocketToPipeWorker(WebSocket socket, IMessagePipeEnd pipe, CancellationToken cancellationToken) {
            const int blockSize = 0x10000;
            var buffer = new MemoryStream(blockSize);

            while (true) {
                cancellationToken.ThrowIfCancellationRequested();

                int index = (int)buffer.Length;
                buffer.SetLength(index + blockSize);
                buffer.TryGetBuffer(out ArraySegment<byte> bufferSegment);

                var wsrr = await socket.ReceiveAsync(new ArraySegment<byte>(bufferSegment.Array, index, blockSize), cancellationToken);
                buffer.SetLength(index + wsrr.Count);

                if (wsrr.CloseStatus != null) {
                    break;
                }

                if (wsrr.EndOfMessage) {
                    pipe.Write(buffer.ToArray());
                    buffer.SetLength(0);
                }
            }
        }

        private static async Task PipeToWebSocketWorker(WebSocket socket, IMessagePipeEnd pipe, CancellationToken cancellationToken) {
            while (true) {
                cancellationToken.ThrowIfCancellationRequested();

                byte[] message;
                try {
                    message = await pipe.ReadAsync(cancellationToken);
                } catch (PipeDisconnectedException) {
                    break;
                }

                await socket.SendAsync(new ArraySegment<byte>(message, 0, message.Length), WebSocketMessageType.Binary, true, cancellationToken);
            }
        }
    }
}

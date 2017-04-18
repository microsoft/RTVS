// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Client {
    internal sealed class WebSocketMessageTransport : IMessageTransport {
        private readonly WebSocket _socket;
        private readonly SemaphoreSlim _receiveLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

        public string CloseStatusDescription => _socket.CloseStatusDescription ?? string.Empty;

        public WebSocketMessageTransport(WebSocket socket) {
            _socket = socket;
        }

        public async Task CloseAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            await _sendLock.WaitAsync(cancellationToken);
            try {
                await _socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", cancellationToken);
            } catch (Exception ex) when (IsTransportException(ex)) {
                throw new MessageTransportException(ex);
            } finally {
                _sendLock.Release();
            }
        }

        public async Task<Message> ReceiveAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            const int blockSize = 0x10000;
            var buffer = new MemoryStream(blockSize);

            while (true) {
                cancellationToken.ThrowIfCancellationRequested();

                int index = (int)buffer.Length;
                buffer.SetLength(index + blockSize);

                await _receiveLock.WaitAsync(cancellationToken);
                WebSocketReceiveResult wsrr;
                try {
                    ArraySegment<byte> arrSegment;
                    buffer.TryGetBuffer(out arrSegment);
                    wsrr = await _socket.ReceiveAsync(new ArraySegment<byte>(arrSegment.Array, index, blockSize), cancellationToken);
                } catch (Exception ex) when(IsTransportException(ex)) {
                    throw new MessageTransportException(ex);
                } finally {
                    _receiveLock.Release();
                }

                buffer.SetLength(index + wsrr.Count);

                if (wsrr.CloseStatus != null) {
                    return null;
                }

                if (wsrr.EndOfMessage) {
                    return Message.Parse(buffer.ToArray());
                }
            }
        }

        public async Task SendAsync(Message message, CancellationToken cancellationToken = default(CancellationToken)) {
            var data = message.ToBytes();
            await _sendLock.WaitAsync(cancellationToken);
            try {
                await _socket.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Binary, true, cancellationToken);
            } catch (Exception ex) when(IsTransportException(ex)) {
                throw new MessageTransportException(ex);
            } finally {
                _sendLock.Release();
            }
        }

        private static bool IsTransportException(Exception ex) {
            return ex is IOException || ex is SocketException || ex is WebSocketException || ex is ObjectDisposedException;
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Broker.Pipes {
    public class MessagePipe {
        private readonly ILogger _logger;
        private int _pid;

        // TODO: this is a bottleneck, since all VS-host traffic flows through the pipe.
        // Need to check if BufferBlock is fast enough, and see if there are any better substitutes if not.
        private readonly BufferBlock<byte[]> _hostMessages = new BufferBlock<byte[]>();
        private readonly BufferBlock<byte[]> _clientMessages = new BufferBlock<byte[]>();

        private ConcurrentDictionary<ulong, byte[]> _sentPendingRequests = new ConcurrentDictionary<ulong, byte[]>();
        private Queue<byte[]> _unsentPendingRequests = new Queue<byte[]>();

        private byte[] _handshake;
        private IMessagePipeEnd _hostEnd;
        private IOwnedMessagePipeEnd _clientEnd;

        private sealed class HostEnd : IMessagePipeEnd {
            private readonly MessagePipe _pipe;

            public HostEnd(MessagePipe pipe) {
                _pipe = pipe;
            }

            public void Write(byte[] message) {
                _pipe.LogMessage(MessageOrigin.Host, message);
                _pipe._hostMessages.Post(message);
            }

            public async Task<byte[]> ReadAsync(CancellationToken cancellationToken) {
                return await _pipe._clientMessages.ReceiveAsync();
            }
        }

        private sealed class ClientEnd : IOwnedMessagePipeEnd {
            private readonly MessagePipe _pipe;
            private bool _isFirstRead = true;

            public ClientEnd(MessagePipe pipe) {
                _pipe = pipe;
            }

            public void Dispose() {
                var unsent = new Queue<byte[]>(_pipe._sentPendingRequests.OrderBy(kv => kv.Key).Select(kv => kv.Value));
                _pipe._sentPendingRequests.Clear();
                Volatile.Write(ref _pipe._unsentPendingRequests, unsent);
                Volatile.Write(ref _pipe._clientEnd, null);
            }

            public void Write(byte[] message) {
                _pipe.LogMessage(MessageOrigin.Client, message);

                ulong id, requestId;
                Parse(message, out id, out requestId);

                byte[] request;
                _pipe._sentPendingRequests.TryRemove(requestId, out request);

                _pipe._clientMessages.Post(message);
            }

            public async Task<byte[]> ReadAsync(CancellationToken cancellationToken) {
                var handshake = _pipe._handshake;
                if (_isFirstRead) {
                    _isFirstRead = false;
                    if (handshake != null) {
                        return handshake;
                    }
                }

                byte[] message;
                if (_pipe._unsentPendingRequests.Count != 0) {
                    message = _pipe._unsentPendingRequests.Dequeue();
                } else {
                    message = await _pipe._hostMessages.ReceiveAsync();
                }

                ulong id, requestId;
                Parse(message, out id, out requestId);

                if (handshake == null) {
                    _pipe._handshake = message;
                } else if (requestId == ulong.MaxValue) {
                    _pipe._sentPendingRequests.TryAdd(id, message);
                }

                return message;
            }
        }

        public MessagePipe(ILogger logger) {
            _logger = logger;
        }

        /// <summary>
        /// Creates and returns the host end of the pipe.
        /// </summary>
        /// <remarks>
        /// Can only be called once for a given instance of <see cref="MessagePipe"/>. The returned
        /// object is owned by the pipe, and should not be disposed.
        /// </remarks>
        public IMessagePipeEnd ConnectHost(int pid) {
            if (Interlocked.CompareExchange(ref _hostEnd, new HostEnd(this), null) != null) {
                throw new InvalidOperationException(Resources.Exception_PipeHasHostEnd);
            }

            _pid = pid;
            return _hostEnd;
        }

        /// <summary>
        /// Creates and returns the client end of the pipe.
        /// </summary>
        /// <remarks>
        /// Can be called multiple times for a given instance of <see cref="MessagePipe"/>, but only
        /// one end can be active at once. The existing end must be disposed before calling this method
        /// to create a new one.
        /// </remarks>
        public IOwnedMessagePipeEnd ConnectClient() {
            if (Interlocked.CompareExchange(ref _clientEnd, new ClientEnd(this), null) != null) {
                throw new InvalidOperationException(Resources.Exception_PipeHasClientEnd);
            }

            return _clientEnd;
        }

        private static void Parse(byte[] message, out ulong id, out ulong requestId) {
            id = BitConverter.ToUInt64(message, 0);
            requestId = BitConverter.ToUInt64(message, 8);
        }

        private enum MessageOrigin {
            Host,
            Client
        }

        private void LogMessage(MessageOrigin origin, byte[] messageData) {
            if (_logger == null) {
                return;
            }

            Message message;
            try {
                message = new Message(messageData);
            } catch (InvalidDataException ex) {
                _logger.Log(LogLevel.Error, 0, messageData, ex, delegate {
                    return $"Malformed {origin.ToString().ToLowerInvariant()} message:{Environment.NewLine}{BitConverter.ToString(messageData)}";
                });
                return;
            }

            _logger.Log(LogLevel.Trace, 0, message, null, delegate {
                var sb = new StringBuilder($"|{_pid}|{(origin == MessageOrigin.Host ? ">" : "<")} #{message.Id}# {message.Name} ");

                if (message.IsResponse) {
                    sb.Append($"#{message.RequestId}# ");
                }

                sb.Append(message.Json);

                if (message.Blob != null && message.Blob.Length != 0) {
                    sb.Append($" <raw ({message.Blob.Length} bytes)>");
                }

                return sb.ToString();
            });
        }
    }
}

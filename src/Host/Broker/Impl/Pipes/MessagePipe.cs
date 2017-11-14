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
using static System.FormattableString;

namespace Microsoft.R.Host.Broker.Pipes {
    public class MessagePipe {
        private static readonly byte[] _cancelAllMessageName = Encoding.ASCII.GetBytes("!//");
        private static readonly byte[] _disconnectMessage = new byte[0];

        private readonly ILogger _logger;
        private int _pid;

        // TODO: this is a bottleneck, since all VS-host traffic flows through the pipe.
        // Need to check if BufferBlock is fast enough, and see if there are any better substitutes if not.
        private readonly BufferBlock<byte[]> _hostMessages = new BufferBlock<byte[]>();
        private readonly BufferBlock<byte[]> _clientMessages = new BufferBlock<byte[]>();

        private ConcurrentDictionary<ulong, byte[]> _sentPendingRequests = new ConcurrentDictionary<ulong, byte[]>();
        private Queue<byte[]> _unsentPendingRequests = new Queue<byte[]>();

        private byte[] _handshake;
        private IMessagePipeEnd _hostEnd, _clientEnd;

        private sealed class HostEnd : IMessagePipeEnd {
            private readonly MessagePipe _pipe;

            public HostEnd(MessagePipe pipe) {
                _pipe = pipe;
            }

            public void Dispose() {
                Volatile.Write(ref _pipe._hostEnd, null);
                _pipe._hostMessages.Post(_disconnectMessage);
            }

            public void Write(byte[] message) {
                _pipe.LogMessage(MessageOrigin.Host, message);
                _pipe._hostMessages.Post(message);
            }

            public async Task<byte[]> ReadAsync(CancellationToken cancellationToken) {
                var message = await _pipe._clientMessages.ReceiveAsync(cancellationToken);
                if (message == _disconnectMessage) {
                    throw new PipeDisconnectedException();
                }
                return message;
            }
        }

        private sealed class ClientEnd : IMessagePipeEnd {
            private readonly MessagePipe _pipe;
            private bool _isFirstRead = true;

            public ClientEnd(MessagePipe pipe) {
                _pipe = pipe;
            }

            public void Dispose() {
                var unsent = new Queue<byte[]>(_pipe._sentPendingRequests
                    .OrderBy(kv => kv.Key)
                    .Select(kv => kv.Value)
                    .Where(msg => msg != _disconnectMessage));
                _pipe._sentPendingRequests.Clear();

                Volatile.Write(ref _pipe._unsentPendingRequests, unsent);
                Volatile.Write(ref _pipe._clientEnd, null);

                _pipe._clientMessages.Post(_disconnectMessage);
            }

            public void Write(byte[] message) {
                _pipe.LogMessage(MessageOrigin.Client, message);

                var requestId = MessageParser.GetRequestId(message);

                if (requestId == 0) {
                    if (MessageParser.IsNamed(message, _cancelAllMessageName)) {
                        _pipe._sentPendingRequests.Clear();
                    }
                } else {
                    _pipe._sentPendingRequests.TryRemove(requestId, out var request);
                }

                _pipe._clientMessages.Post(message);
            }

            public async Task<byte[]> ReadAsync(CancellationToken cancellationToken) {
                if (Volatile.Read(ref _pipe._hostEnd) == null) {
                    throw new PipeDisconnectedException();
                }

                var handshake = _pipe._handshake;
                if (_isFirstRead) {
                    _isFirstRead = false;
                    if (handshake != null) {
                        _pipe.LogMessage(MessageOrigin.Host, handshake, replay: true);
                        return handshake;
                    }
                }

                byte[] message;
                if (_pipe._unsentPendingRequests.Count != 0) {
                    message = _pipe._unsentPendingRequests.Dequeue();
                    _pipe.LogMessage(MessageOrigin.Host, message, replay: true);
                } else {
                    message = await _pipe._hostMessages.ReceiveAsync(cancellationToken);
                }

                if (message == _disconnectMessage) {
                    throw new PipeDisconnectedException();
                } else if (handshake == null) {
                    _pipe._handshake = message;
                } else {
                    var requestId = MessageParser.GetRequestId(message);
                    if (requestId == ulong.MaxValue) {
                        var id = MessageParser.GetId(message);
                        _pipe._sentPendingRequests.TryAdd(id, message);
                    }
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
        public IMessagePipeEnd ConnectClient() {
            if (Interlocked.CompareExchange(ref _clientEnd, new ClientEnd(this), null) != null) {
                throw new InvalidOperationException(Resources.Exception_PipeHasClientEnd);
            }

            return _clientEnd;
        }

        private enum MessageOrigin {
            Host,
            Client
        }

        private void LogMessage(MessageOrigin origin, byte[] messageData, bool replay = false) {
            if (_logger == null) {
                return;
            }

            Message message;
            try {
                message = Message.Parse(messageData);
            } catch (InvalidDataException ex) {
                _logger.Log(LogLevel.Error, 0, messageData, ex, delegate {
                    return Invariant($"Malformed {origin.ToString().ToLowerInvariant()} message:{Environment.NewLine}{BitConverter.ToString(messageData)}");
                });
                return;
            }

            _logger.Log(LogLevel.Trace, 0, message, null, delegate {
                var sb = new StringBuilder(replay ? "(replay) " : "");

                sb.Append(Invariant($"|{_pid}|{(origin == MessageOrigin.Host ? ">" : "<")} #{message.Id}# {message.Name} "));

                if (message.IsResponse) {
                    sb.Append(Invariant($"#{message.RequestId}# "));
                }

                sb.Append(message.Json);

                if (message.Blob != null && message.Blob.Length != 0) {
                    sb.Append(Invariant($" <raw ({message.Blob.Length} bytes)>"));
                }

                return sb.ToString();
            });
        }
    }
}

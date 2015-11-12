using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace Microsoft.R.Host.Client {
    public class WebSocketMessageTransport : WebSocketBehavior, IMessageTransport {
        private readonly WebSocket _socket;
        private readonly BufferBlock<Task<string>> _incomingMessages = new BufferBlock<Task<string>>();
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);

        private WebSocket Socket => _socket ?? Context.WebSocket;

        public event EventHandler Open;
        public event EventHandler Close;

        /// <remarks>
        /// 
        /// </remarks>
        public WebSocketMessageTransport() {
            Protocol = "Microsoft.R.Host";
        }

        public WebSocketMessageTransport(WebSocket socket) : this() {
            _socket = socket;
            _socket.OnError += (sender, args) => OnError(args);
            _socket.OnMessage += (sender, args) => OnMessage(args);
            _socket.OnClose += (sender, args) => OnClose(args);
        }

        public async Task<string> ReceiveAsync(CancellationToken ct = default(CancellationToken)) {
            return await await _incomingMessages.ReceiveAsync(ct);
        }

        public async Task SendAsync(string message, CancellationToken ct = default(CancellationToken)) {
            await _sendLock.WaitAsync(ct);
            try {
                var tcs = new TaskCompletionSource<object>();
                Socket.SendAsync(message, ok => {
                    if (ok) {
                        tcs.SetResult(null);
                    } else {
                        tcs.SetException(new MessageTransportException("Websocket send failed."));
                    }
                });
                await tcs.Task;
            } catch (SocketException ex) {
                throw new MessageTransportException(ex);
            } catch (WebSocketException ex) {
                throw new MessageTransportException(ex);
            } finally {
                _sendLock.Release();
            }
        }

        protected override void OnOpen() {
            base.OnOpen();
            Open?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnClose(CloseEventArgs e) {
            base.OnClose(e);
            _incomingMessages.Post(Task.FromException<string>(new OperationCanceledException("Connection closed by host.")));
            Close?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnMessage(MessageEventArgs e) {
            base.OnMessage(e);
            _incomingMessages.Post(Task.FromResult(e.Data));
        }

        protected override void OnError(ErrorEventArgs e) {
            base.OnError(e);

            var ex = e.Exception;
            if (ex == null) {
                ex = new MessageTransportException();
            } else if (ex is SocketException || ex is WebSocketException) {
                ex = new MessageTransportException(ex);
            } 

            _incomingMessages.Post(Task.FromException<string>(ex));
        }
    }
}

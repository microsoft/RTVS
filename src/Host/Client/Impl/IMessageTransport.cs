using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    public interface IMessageTransport {
        Task SendAsync(string message, CancellationToken ct = default(CancellationToken));
        Task<string> ReceiveAsync(CancellationToken ct = default(CancellationToken));
    }

    [Serializable]
    public class MessageTransportException : Exception {
        public MessageTransportException() {
        }

        public MessageTransportException(string message)
            : base(message) {
        }

        public MessageTransportException(string message, Exception innerException)
            : base(message, innerException) {
        }

        public MessageTransportException(Exception innerException)
            : this(innerException.Message, innerException) {
        }
    }
}

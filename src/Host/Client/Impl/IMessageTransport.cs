using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.R.Host.Client {
    public interface IMessageTransport {
        Task SendAsync(string message, CancellationToken ct = default(CancellationToken));
        Task<string> ReceiveAsync(CancellationToken ct = default(CancellationToken));
    }
}

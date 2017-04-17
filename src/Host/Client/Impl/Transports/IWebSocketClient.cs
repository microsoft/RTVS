using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Text;
using System.Threading;

namespace Microsoft.R.Host.Client {
    public interface IWebSocketClient {
        IList<string> SubProtocols { get; }
        TimeSpan KeepAliveInterval { get; set; }
        int ReceiveBufferSize { get; set; }
        bool UseZeroMask { get; set; }
        Action<HttpWebResponse> InspectResponse { get; set; }

        HttpWebRequest CreateRequest(Uri uri, ICredentials serverCredentials);
        Task<WebSocket> ConnectAsync(HttpWebRequest request, CancellationToken cancellationToken);
    }
}

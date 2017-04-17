

using System.Collections.Generic;

namespace Microsoft.R.Host.Client {
    public interface IWebSocketClientService {
        IWebSocketClient Create(IList<string> subProtocols);
    }
}

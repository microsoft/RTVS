using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebSockets.Client;

namespace Microsoft.R.Host.Client {
    public class WebSocketClientService : IWebSocketClientService {
        public IWebSocketClient Create(IList<string> subProtocols) {
            return new WebSocketClient(subProtocols);
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.WebSockets.Client;

namespace Microsoft.R.Host.Client {
    public class WebSocketClientService : IWebSocketClientService {
        public IWebSocketClient Create(IList<string> subProtocols) {
            return new WebSocketClient(subProtocols);
        }
    }
}

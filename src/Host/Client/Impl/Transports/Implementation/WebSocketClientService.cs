// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.WebSockets.Client;

namespace Microsoft.R.Host.Client {
    internal sealed class WebSocketClientService : IWebSocketClientService {
        public IWebSocketClient Create(IList<string> subProtocols) => new WebSocketClient(subProtocols);
    }
}

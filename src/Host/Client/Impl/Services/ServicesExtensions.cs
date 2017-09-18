// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Services;

namespace Microsoft.R.Host.Client {
    public static class ServicesExtensions {
        public static IServiceManager AddHostClientServices(this IServiceManager serviceManager) => serviceManager
            .AddService<IWebSocketClientService, WebSocketClientService>();
    }
}

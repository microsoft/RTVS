// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Services;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.ConnectionManager.Implementation;
using Microsoft.R.Components.History;
using Microsoft.R.Components.History.Implementation;

namespace Microsoft.R.Components {
    public static class ServicesExtensions {
        public static IServiceManager AddWindowsRComponentstServices(this IServiceManager serviceManager) => serviceManager
            .AddService<IConnectionManagerProvider, ConnectionManagerProvider>()
            .AddService<IRHistoryProvider, RHistoryProvider>();
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Services;
using Microsoft.R.Containers.Docker;

namespace Microsoft.R.Containers {
    public static class ServicesExtensions {
        public static IServiceManager AddWindowsContainerServices(this IServiceManager serviceManager) => serviceManager
            .AddService<IContainerService, WindowsDockerService>();
    }
}

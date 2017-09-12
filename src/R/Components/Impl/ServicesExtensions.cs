// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.Services;
using Microsoft.R.Components.Containers;
using Microsoft.R.Components.Containers.Implementation;

namespace Microsoft.R.Components {
    public static class ServicesExtensions {
        public static IServiceManager AddRComponentsServices(this IServiceManager serviceManager) => serviceManager
            .AddService<IContainerManagerProvider, ContainerManagerProvider>();
    }
}

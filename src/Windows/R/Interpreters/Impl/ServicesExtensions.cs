// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;

namespace Microsoft.R.Interpreters {
    public static class ServicesExtensions {
        public static IServiceManager AddWindowsRInterpretersServices(this IServiceManager serviceManager) {
            return serviceManager
                .AddService<IRInstallationService>(new RInstallation());
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.InteropServices;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using Microsoft.R.Platform.Interpreters;
using Microsoft.R.Platform.Interpreters.Linux;
using Microsoft.R.Platform.Interpreters.Mac;
using Microsoft.R.Platform.IO;
using Microsoft.R.Platform.Logging;

namespace Microsoft.R.Platform {
    /// <summary>
    /// Invoked via reflection to populate service container
    /// with platform-specific services such as R discovery,
    /// file system, process management.
    /// </summary>
    public static class ServiceProvider {
        public static void ProvideServices(IServiceManager services) {
            IRInstallationService installation;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                installation = new RMacInstallation();
            } else {
                installation = new RLinuxInstallation();
            }

            services
                .AddService(new UnixProcessServices())
                .AddService(new UnixFileSystem())
                .AddService(new UnixLoggingPermissions())
                .AddService(installation);
        }
    }
}

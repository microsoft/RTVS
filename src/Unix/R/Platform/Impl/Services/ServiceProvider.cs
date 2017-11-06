// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.InteropServices;
using Microsoft.Common.Core.OS.Linux;
using Microsoft.Common.Core.OS.Mac;
using Microsoft.Common.Core.Services;
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
            services
                .AddService(new UnixFileSystem())
                .AddService(new UnixLoggingPermissions());

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                services
                    .AddService(new MacProcessServices())
                    .AddService(new RMacInstallation())
                    .AddService(new MacPlatformServices());
            } else {
                services
                    .AddService(new LinuxProcessServices())
                    .AddService(new RLinuxInstallation());
            }
        }
    }
}

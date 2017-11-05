// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Runtime.InteropServices;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.OS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Broker.Services;
using Microsoft.R.Platform.Interpreters;
using Microsoft.R.Platform.Interpreters.Linux;
using Microsoft.R.Platform.Interpreters.Mac;
using Microsoft.R.Platform.IO;

namespace Microsoft.R.Host.Broker.Start {
    public sealed class UnixStartup : Startup {
        public UnixStartup(ILoggerFactory loggerFactory, IConfigurationRoot configuration) : base(loggerFactory, configuration) { }

        public override void ConfigureServices(IServiceCollection services) {
            base.ConfigureServices(services);

            services
                .AddSingleton<IFileSystem, UnixFileSystem>()
                .AddSingleton<IProcessServices, UnixProcessServices>()
                .AddSingleton<IPlatformAuthenticationService, LinuxAuthenticationService>()
                .AddSingleton<ISystemInfoService, LinuxSystemInfoService>();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                services
                    .AddSingleton<IRInstallationService, RMacInstallation>()
                    .AddSingleton<IRHostProcessService, MacRHostProcessService>();
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                services
                    .AddSingleton<IRInstallationService, RLinuxInstallation>()
                    .AddSingleton<IRHostProcessService, LinuxRHostProcessService>();
            } else {
                throw new NotSupportedException("Platform is not supported. Supported: Linux, OSX");
            }
        }
    }
}
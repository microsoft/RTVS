// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Runtime.InteropServices;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.OS.Linux;
using Microsoft.Common.Core.OS.Mac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Broker.Services;
using Microsoft.R.Host.Broker.Services.Linux;
using Microsoft.R.Host.Broker.Services.Mac;
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
                .AddSingleton<IPlatformAuthenticationService, LinuxAuthenticationService>();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                services
                    .AddSingleton<IProcessServices, MacProcessServices>()
                    .AddSingleton<IRInstallationService, RMacInstallation>()
                    .AddSingleton<IRHostProcessService, MacRHostProcessService>()
                    .AddSingleton<ISystemInfoService, MacSystemInfoService>();
            } else {
                services
                    .AddSingleton<IProcessServices, LinuxProcessServices>()
                    .AddSingleton<IRInstallationService, RLinuxInstallation>()
                    .AddSingleton<IRHostProcessService, LinuxRHostProcessService>()
                    .AddSingleton<ISystemInfoService, LinuxSystemInfoService>();
            }
        }
    }
}
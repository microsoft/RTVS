// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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

namespace Microsoft.R.Host.Broker.Startup {
    public sealed class UnixStartup : Startup {
        public UnixStartup(ILoggerFactory loggerFactory, IConfigurationRoot configuration) : base(loggerFactory, configuration) { }

        public override void ConfigureServices(IServiceCollection services) {
            base.ConfigureServices(services);

            IRInstallationService installation;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                installation = new RMacInstallation();
            } else {
                installation = new RLinuxInstallation();
            }

            services.AddSingleton<IFileSystem, UnixFileSystem>()
                .AddSingleton<IProcessServices, UnixProcessServices>()
                .AddSingleton<IAuthenticationService, LinuxAuthenticationService>()
                .AddSingleton<IRHostProcessService, LinuxRHostProcessService>()
                .AddSingleton(installation)
                .AddSingleton<ISystemInfoService, LinuxSystemInfoService>();
        }
    }
}
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Broker.Services;
using Microsoft.R.Interpreters;
using Microsoft.R.Common.Core.Linux;

namespace Microsoft.R.Host.Broker.Startup
{
    public sealed class LinuxStartup : Startup {
        public LinuxStartup(ILoggerFactory loggerFactory, IConfigurationRoot configuration) : base(loggerFactory, configuration) { }

        public override void ConfigureServices(IServiceCollection services) {
            base.ConfigureServices(services);

            var fs = new FileSystem();
            services.AddSingleton<IFileSystem>(fs)
                .AddSingleton<IAuthenticationService, LinuxAuthenticationService>()
                .AddSingleton<IRHostProcessService, LinuxRHostProcessService>()
                .AddSingleton<IRInstallationService, RInstallation>()
                .AddSingleton<ISystemInfoService>(new LinuxSystemInfoService(fs));
        }
    }
}
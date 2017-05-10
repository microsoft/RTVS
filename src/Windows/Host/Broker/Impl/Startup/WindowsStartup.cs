// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Broker.Services;
using Microsoft.R.Interpreters;

namespace Microsoft.R.Host.Broker.Startup {
    public sealed class WindowsStartup : Startup {
        public WindowsStartup(ILoggerFactory loggerFactory, IConfigurationRoot configuration) : base(loggerFactory, configuration) {}

        public override void ConfigureServices(IServiceCollection services) {
            base.ConfigureServices(services);

            services.AddSingleton<IFileSystem>(new WindowsFileSystem())
                .AddSingleton<IAuthenticationService, WindowsAuthenticationService>()
                .AddSingleton<IRHostProcessService, WindowsRHostProcessService>()
                .AddSingleton<IRInstallationService, RInstallation>()
                .AddSingleton<ISystemInfoService, WindowsSystemInfoService>();
        }
    }
}

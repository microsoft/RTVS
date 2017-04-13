// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Common.Core.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.R.Host.Broker.Interpreters;
using Microsoft.R.Host.Broker.Lifetime;
using Microsoft.R.Host.Broker.Logging;
using Microsoft.R.Host.Broker.Security;
using Microsoft.R.Host.Broker.Services;
using Microsoft.R.Interpreters;

namespace Microsoft.R.Host.Broker.Startup {
    public sealed class ServiceStartup {
        public ServiceStartup(IHostingEnvironment env) { }

        public void ConfigureServices(IServiceCollection services) { }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, LifetimeManager lifetimeManager, InterpreterManager interpreterManager, SecurityManager securityManager) { }
    }

    public sealed class StandaloneStartup {
        public StandaloneStartup(IHostingEnvironment env) { }

        public void ConfigureServices(IServiceCollection services) {
            Startup.ConfigureServices(services);
            WindowsStartup.ConfigureServices(services);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, LifetimeManager lifetimeManager, InterpreterManager interpreterManager, SecurityManager securityManager) {
            Startup.Configure(app, env, lifetimeManager, interpreterManager, securityManager);
        }
    }

    public static class WindowsStartup {
        public static void ConfigureServices(IServiceCollection services) {
            services.AddOptions()
                .Configure<LoggingOptions>(CommonStartup.Configuration.GetSection("logging"))
                .Configure<LifetimeOptions>(CommonStartup.Configuration.GetSection("lifetime"))
                .Configure<SecurityOptions>(CommonStartup.Configuration.GetSection("security"))
                .Configure<ROptions>(CommonStartup.Configuration.GetSection("R"));

            services.AddSingleton<IFileSystem>(new FileSystem())
                    .AddSingleton<IAuthenticationService, WindowsAuthenticationService>()
                    .AddSingleton<IRHostProcessService, WindowsRHostProcessService>()
                    .AddSingleton<IRInstallationService, RInstallation>()
                    .AddSingleton<ISystemInfoService, WindowsSystemInfoService>()
                    .AddSingleton<IExitService, ExitService>();
        }
    }
}

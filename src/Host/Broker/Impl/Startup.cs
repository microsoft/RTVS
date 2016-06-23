// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Broker.Interpreters;
using Microsoft.R.Host.Broker.Security;

namespace Microsoft.R.Host.Broker {
    public class Startup {
        public Startup(IHostingEnvironment env) {
        }

        public void ConfigureServices(IServiceCollection services) {
            services.AddOptions()
                .Configure<InterpretersOptions>(Program.Configuration.GetSection("Interpreters"))
                .Configure<SecurityOptions>(Program.Configuration.GetSection("Security"));

            services.AddSingleton<IAuthorizationHandler, RUserAuthorizationHandler>();

            services.AddAuthorization(options => options.AddPolicy(Policies.RUser, policy =>
                policy.AddRequirements(new RUserAuthorizationRequirement())));

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory) {
            loggerFactory.AddConsole(LogLevel.Trace);
            loggerFactory.AddDebug();

            app.UseWebSockets();
            app.UseMvc();
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Common.Core.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.R.Host.Broker.About;
using Microsoft.R.Host.Broker.Interpreters;
using Microsoft.R.Host.Broker.Lifetime;
using Microsoft.R.Host.Broker.Logging;
using Microsoft.R.Host.Broker.RemoteUri;
using Microsoft.R.Host.Broker.Security;
using Microsoft.R.Host.Broker.Sessions;
using Microsoft.R.Host.Broker.UserProfile;
using Odachi.AspNetCore.Authentication.Basic;

namespace Microsoft.R.Host.Broker.Startup {
    public static class Startup {
        public static void ConfigureServices(IServiceCollection services) {
            services
                .AddSingleton<LifetimeManager>()
                .AddSingleton<SecurityManager>()
                .AddSingleton<InterpreterManager>()
                .AddSingleton<SessionManager>()
                .AddSingleton<UserProfileManager>();

            services.AddAuthorization(options => options.AddPolicy(
                Policies.RUser,
                policy => policy.RequireClaim(Claims.RUser)));

            services.AddRouting();

            services
                .AddMvc()
                .AddApplicationPart(typeof(Startup).GetTypeInfo().Assembly);
        }

        public static void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            LifetimeManager lifetimeManager,
            InterpreterManager interpreterManager,
            SecurityManager securityManager
        ) {
            lifetimeManager.Initialize();
            interpreterManager.Initialize();

            app.UseWebSockets(new WebSocketOptions {
                ReplaceFeature = true,
                KeepAliveInterval = TimeSpan.FromMilliseconds(1000000000),
                ReceiveBufferSize = 0x10000
            });

            var routeBuilder = new RouteBuilder(app, new RouteHandler(RemoteUriHelper.HandlerAsync));
            routeBuilder.MapRoute("help_and_shiny", "remoteuri");
            app.UseRouter(routeBuilder.Build());

            app.UseBasicAuthentication(options => options.Events = new BasicEvents { OnSignIn = securityManager.SignInAsync });

            app.Use((context, next) => context.User.Identity.IsAuthenticated 
                ? next() 
                : context.Authentication.ChallengeAsync());

            app.UseMvc();
        }
    }
}

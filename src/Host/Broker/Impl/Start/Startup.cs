// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Common.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.R.Host.Broker.Interpreters;
using Microsoft.R.Host.Broker.Lifetime;
using Microsoft.R.Host.Broker.Logging;
using Microsoft.R.Host.Broker.RemoteUri;
using Microsoft.R.Host.Broker.Security;
using Microsoft.R.Host.Broker.Sessions;
using Microsoft.R.Host.Protocol;
using Newtonsoft.Json;
using Odachi.AspNetCore.Authentication.Basic;

namespace Microsoft.R.Host.Broker.Start {
    public abstract class Startup : StartupBase {
        private ILogger<Startup> _logger;
        protected ILoggerFactory LoggerFactory { get; }
        protected IConfigurationRoot Configuration { get; }

        protected Startup(ILoggerFactory loggerFactory, IConfigurationRoot configuration) {
            LoggerFactory = loggerFactory;
            Configuration = configuration;
        }

        public override void ConfigureServices(IServiceCollection services) {
            base.ConfigureServices(services);
            services
                .AddOptions()
                .Configure<LifetimeOptions>(Configuration.GetLifetimeSection())
                .Configure<LoggingOptions>(Configuration.GetLoggingSection())
                .Configure<ROptions>(Configuration.GetRSection())
                .Configure<SecurityOptions>(Configuration.GetSecuritySection())
                .Configure<StartupOptions>(Configuration.GetStartupSection())

                .AddSingleton<LifetimeManager>()
                .AddSingleton<SecurityManager>()
                .AddSingleton<InterpreterManager>()
                .AddSingleton<SessionManager>()

                .AddRouting()
                .AddAuthorization(options => 
                    options.AddPolicy(Policies.RUser, policy => {
                    policy.RequireClaim(Claims.RUser);
                    policy.AddAuthenticationSchemes(new[] { BasicDefaults.AuthenticationScheme });
                }))

                .AddMvc(config => {
                     var policy = new AuthorizationPolicyBuilder(new[] { BasicDefaults.AuthenticationScheme })
                                     .RequireClaim(Claims.RUser)
                                     .Build();
                     config.Filters.Add(new AuthorizeFilter(policy));
                 })
                .AddApplicationPart(typeof(SessionsController).GetTypeInfo().Assembly);

            services
                .AddAuthentication(BasicDefaults.AuthenticationScheme)
                .AddBasic();
        }

        public override void Configure(IApplicationBuilder app) {
            var applicationLifetime = app.ApplicationServices.GetService<IApplicationLifetime>();
            var env = app.ApplicationServices.GetService<IHostingEnvironment>();
            var startupOptions = app.ApplicationServices.GetService<IOptions<StartupOptions>>();
            var logger = app.ApplicationServices.GetService<ILogger<Startup>>();
            var lifetimeManager = app.ApplicationServices.GetService<LifetimeManager>();
            var interpreterManager = app.ApplicationServices.GetService<InterpreterManager>();
            var securityManager = app.ApplicationServices.GetService<SecurityManager>();

            _logger = logger;
            var serverAddresses = app.ServerFeatures.Get<IServerAddressesFeature>();
            var pipeName = startupOptions.Value.WriteServerUrlsToPipe;
            if (pipeName != null) {
                NamedPipeClientStream pipe;
                try {
                    pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
                    pipe.Connect(Debugger.IsAttached ? 200000 : 10000);
                } catch (IOException ex) {
                    logger.LogCritical(0, ex, Resources.Critical_InvalidPipeHandle, pipeName);
                    throw;
                } catch (TimeoutException ex) {
                    logger.LogCritical(0, ex, Resources.Critical_PipeConnectTimeOut, pipeName);
                    throw;
                }

                applicationLifetime.ApplicationStarted.Register(() => Task.Run(() => {
                    using (pipe) {
                        var serverUriStr = JsonConvert.SerializeObject(serverAddresses.Addresses);
                        logger.LogTrace(Resources.Trace_ServerUrlsToPipeBegin, pipeName, Environment.NewLine, serverUriStr);

                        var serverUriData = Encoding.UTF8.GetBytes(serverUriStr);
                        pipe.Write(serverUriData, 0, serverUriData.Length);

                        var terminator = Encoding.UTF8.GetBytes("^");
                        pipe.Write(terminator, 0, terminator.Length);

                        pipe.Flush();
                    }

                    logger.LogTrace(Resources.Trace_ServerUrlsToPipeDone, pipeName);
                }));
            }

            lifetimeManager.Initialize();
            interpreterManager.Initialize();

            app.UseWebSockets(new WebSocketOptions {
                KeepAliveInterval = TimeSpan.FromMilliseconds(1000000000),
                ReceiveBufferSize = 0x10000
            });

            app.UseMvc();
            app.UseAuthentication();

            var routeBuilder = new RouteBuilder(app, new RouteHandler(RemoteUriHelper.HandlerAsync));
            routeBuilder.MapRoute("help_and_shiny", "remoteuri");
            app.UseRouter(routeBuilder.Build());

            if (!startupOptions.Value.IsService) {
                applicationLifetime.ApplicationStopping.Register(ExitAfterTimeout);
            }
        }

        private void ExitAfterTimeout() => ExitAfterTimeoutAsync().DoNotWait();

        private async Task ExitAfterTimeoutAsync() {
            await Task.Delay(10000);
            _logger.LogCritical(Resources.Critical_TimeOutShutdown);
            Environment.Exit((int)BrokerExitCodes.Timeout);
        }
    }
}

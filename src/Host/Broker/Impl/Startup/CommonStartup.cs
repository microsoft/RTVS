// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Broker.Logging;
using Microsoft.R.Host.Broker.Security;
using Microsoft.R.Host.Protocol;
using Newtonsoft.Json;
using System.Diagnostics;
using System.ServiceProcess;

namespace Microsoft.R.Host.Broker.Startup {
    internal class CommonStartup {
        private static readonly ILoggerFactory _loggerFactory = new LoggerFactory();
        private static ILogger _logger;
        private static readonly StartupOptions _startupOptions = new StartupOptions();
        private static readonly SecurityOptions _securityOptions = new SecurityOptions();
        private static readonly LoggingOptions _loggingOptions = new LoggingOptions();
        private static readonly CancellationTokenSource _cts = new CancellationTokenSource();

        internal static IConfigurationRoot Configuration { get; set; }

        internal static CancellationToken CancellationToken => _cts.Token;

        internal static void CommonStartupInit(IConfigurationRoot configuration) {
            Configuration = configuration;
            Configuration.GetSection("startup").Bind(_startupOptions);
            Configuration.GetSection("security").Bind(_securityOptions);
            Configuration.GetSection("logging").Bind(_loggingOptions);

            _loggerFactory
                .AddDebug()
                .AddConsole(LogLevel.Trace)
                .AddProvider(new FileLoggerProvider(_startupOptions.Name, _loggingOptions.LogFolder));
            _logger = _loggerFactory.CreateLogger<Program>();

            if (_startupOptions.Name != null) {
                _logger.LogInformation(Resources.Info_BrokerName, _startupOptions.Name);
            }
        }

        internal static void StartApp(IConfigurationRoot configuration) {
            var tlsConfig = new TlsConfiguration(_logger, _securityOptions);
            var httpsOptions = tlsConfig.GetHttpsOptions(Configuration);

            Uri uri = GetServerUri(Configuration);
            try {
                CreateWebHost(httpsOptions).Run();
            } catch (AggregateException ex) when (ex.IsPortInUseException()) {
                _logger.LogError(0, ex.InnerException, Resources.Error_ConfiguredPortNotAvailable, uri?.Port);
            }
        }

        private static Uri GetServerUri(IConfigurationRoot configuration) {
            try {
                Uri uri;
                var url = configuration.GetValue<string>("server.urls", null);
                if (Uri.TryCreate(url, UriKind.Absolute, out uri) && uri.Port != 0) {
                    return uri;
                }
            } catch (Exception) { }
            return null;
        }

        internal static void StartService(IConfigurationRoot configuration) {
#if DEBUG
            // Uncomment line below to debug service
            // Debugger.Launch();
#endif
            ServiceBase.Run(new ServiceBase[] { new BrokerService() });
        }

        internal static void CreateAndRunWebHostForService() {
            var tlsConfig = new TlsConfiguration(_logger, _securityOptions);
            var httpsOptions = tlsConfig.GetHttpsOptions(Configuration);

            Uri uri = GetServerUri(Configuration);
            try {
                CreateWebHost(httpsOptions).Run();
            } catch (AggregateException ex) when (ex.IsPortInUseException()) {
                _logger.LogError(0, ex.InnerException, Resources.Error_ConfiguredPortNotAvailable, uri?.Port);
            }
        }

        internal static IWebHost CreateWebHost(HttpsConnectionFilterOptions httpsOptions) {

            var webHostBuilder = new WebHostBuilder()
                .UseLoggerFactory(_loggerFactory)
                .UseConfiguration(Configuration)
                .UseKestrel(options => {
                    if (httpsOptions != null) {
                        options.UseHttps(httpsOptions);
                    }
                    //options.UseConnectionLogging();
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>();

            var webHost = webHostBuilder.Build();
            var serverAddresses = webHost.ServerFeatures.Get<IServerAddressesFeature>();

            string pipeName = _startupOptions.WriteServerUrlsToPipe;
            if (pipeName != null) {
                NamedPipeClientStream pipe;
                try {
                    pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.Out);
                    pipe.Connect(10000);
                } catch (IOException ex) {
                    _logger.LogCritical(0, ex, Resources.Critical_InvalidPipeHandle, pipeName);
                    throw;
                } catch (System.TimeoutException ex) {
                    _logger.LogCritical(0, ex, Resources.Critical_PipeConnectTimeOut, pipeName);
                    throw;
                }

                var applicationLifetime = webHost.Services.GetService<IApplicationLifetime>();
                applicationLifetime.ApplicationStarted.Register(() => Task.Run(() => {
                    using (pipe) {
                        string serverUriStr = JsonConvert.SerializeObject(serverAddresses.Addresses);
                        _logger.LogTrace(Resources.Trace_ServerUrlsToPipeBegin, pipeName, Environment.NewLine, serverUriStr);

                        var serverUriData = Encoding.UTF8.GetBytes(serverUriStr);
                        pipe.Write(serverUriData, 0, serverUriData.Length);
                        pipe.Flush();
                    }

                    _logger.LogTrace(Resources.Trace_ServerUrlsToPipeDone, pipeName);
                }));
            }

            return webHost;
        }

        internal static void Exit() {
            _cts.Cancel();

            Task.Run(async () => {
                // Give cooperative cancellation 10 seconds to shut the process down gracefully,
                // but if it didn't work, just terminate it.
                await Task.Delay(10000);
                _logger.LogCritical(Resources.Critical_TimeOutShutdown);
                Environment.Exit((int)BrokerExitCodes.Timeout);
            });
        }
    }
}

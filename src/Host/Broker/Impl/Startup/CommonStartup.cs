// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.IO.Pipes;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Broker.Logging;
using Microsoft.R.Host.Broker.Security;
using Microsoft.R.Host.Protocol;
using Newtonsoft.Json;

namespace Microsoft.R.Host.Broker.Startup {
    internal class CommonStartup {
        private static readonly ILoggerFactory _loggerFactory = new LoggerFactory();
        private static ILogger _logger;
        private static readonly StartupOptions _startupOptions = new StartupOptions();
        private static readonly SecurityOptions _securityOptions = new SecurityOptions();
        private static readonly LoggingOptions _loggingOptions = new LoggingOptions();
        private static readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private static bool IsService { get; set; }

        internal static IConfigurationRoot Configuration { get; set; }

        internal static CancellationToken CancellationToken => _cts.Token;
        internal static ILogger Logger => _logger;

        internal static void Start(IConfigurationRoot configuration, bool isService) {
            IsService = isService;
            if (isService) {
                StartService(configuration);
            } else {
                StartApp(configuration);
            }
        }

        private static void ApplyCommonConfiguration(IConfigurationRoot configuration, EventLogLoggerProvider eventLogProvider = null) {
            // This is needed because we are trying to read the config file. At this point we don't have the config info for the logger 
            // we generally use in the broker. Log will be visible if the user runs broker interactively for broker running locally. Log 
            // will be added to windows event log for the remote broker.
            using (ILoggerFactory configLoggerFactory = new LoggerFactory()) {
                configLoggerFactory
                    .AddDebug()
                    .AddConsole(LogLevel.Trace);

                if (eventLogProvider != null) {
                    configLoggerFactory.AddProvider(eventLogProvider);
                }

                ILogger logger = configLoggerFactory.CreateLogger<BrokerService>();
                try {
                    string configFile = configuration["config"];
                    if (configFile != null) {
                        var configBuilder = new ConfigurationBuilder().AddJsonFile(configFile, optional: false);
                        configuration = configBuilder.Build();
                    }

                    Configuration = configuration;
                    Configuration.GetSection("startup").Bind(_startupOptions);
                    Configuration.GetSection("security").Bind(_securityOptions);
                    Configuration.GetSection("logging").Bind(_loggingOptions);
                } catch (Exception ex) when (!ex.IsCriticalException()) {
                    logger.LogCritical(Resources.Error_ConfigFailed, ex.Message);
                    Exit((int)BrokerExitCodes.BadConfigFile, Resources.Error_ConfigFailed, ex.Message);
                }
            }
        }

        private static ILogger GetLogger(ILoggerFactory loggerFactory, string name) {
            var logger = loggerFactory.CreateLogger<Program>();

            if (name != null) {
                logger.LogInformation(Resources.Info_BrokerName, name);
            }

            return logger;
        }

        private static void StartApp(IConfigurationRoot configuration) {
            ApplyCommonConfiguration(configuration);

            _loggerFactory
                .AddDebug()
                .AddConsole(LogLevel.Trace)
                .AddProvider(new FileLoggerProvider(_startupOptions.Name, _loggingOptions.LogFolder));

            _logger = GetLogger(_loggerFactory, _startupOptions.Name);

            var tlsConfig = new TlsConfiguration(_logger, _securityOptions);
            var httpsOptions = tlsConfig.GetHttpsOptions(Configuration);

            Uri uri = GetServerUri(Configuration);
            try {
                CreateWebHost(httpsOptions).Run(CancellationToken);
            } catch (AggregateException ex) when (ex.IsPortInUseException()) {
                _logger.LogError(0, ex.InnerException, Resources.Error_ConfiguredPortNotAvailable, uri?.Port);
                Exit((int)BrokerExitCodes.PortInUse, null);
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

        private static void StartService(IConfigurationRoot configuration) {
#if DEBUG
            // Uncomment line below to debug service
            // Debugger.Launch();
#endif
            ServiceBase.Run(new ServiceBase[] { new BrokerService(configuration) });
        }

        internal static void CreateAndRunWebHostForService(IConfigurationRoot configuration) {
            ApplyCommonConfiguration(configuration, new EventLogLoggerProvider(LogLevel.Warning, Resources.Text_ServiceName));

            _loggerFactory
                .AddDebug()
                .AddConsole(LogLevel.Trace)
                .AddProvider(new FileLoggerProvider(_startupOptions.Name, _loggingOptions.LogFolder));

            // Add service event logs in addition to file logs.
            _loggerFactory.AddProvider(new EventLogLoggerProvider(LogLevel.Warning, Resources.Text_ServiceName));
            _logger = GetLogger(_loggerFactory, _startupOptions.Name);

            var tlsConfig = new TlsConfiguration(_logger, _securityOptions);
            var httpsOptions = tlsConfig.GetHttpsOptions(Configuration);

            Uri uri = GetServerUri(Configuration);
            try {
                CreateWebHost(httpsOptions).Run(CancellationToken);
            } catch (AggregateException ex) when (ex.IsPortInUseException()) {
                _logger.LogError(0, ex.InnerException, Resources.Error_ConfiguredPortNotAvailable, uri?.Port);
                Exit((int)BrokerExitCodes.PortInUse, null);
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

        private static void ServiceExit() {
            using (var serviceController = new ServiceController(Resources.Text_ServiceName)) {
                serviceController.Stop();
            }
        }

        internal static void Exit() {
            _cts?.Cancel();

            Task.Run(async () => {
                if (IsService) {
                    ServiceExit();
                } else {
                    // Give cooperative cancellation 10 seconds to shut the process down gracefully,
                    // but if it didn't work, just terminate it.
                    await Task.Delay(10000);
                    _logger?.LogCritical(Resources.Critical_TimeOutShutdown);
                    Environment.Exit((int)BrokerExitCodes.Timeout);
                }
            });
        }

        internal static void Exit(int errorCode, string logMessage, params object[] logArgs) {
            _cts?.Cancel();

            if (!string.IsNullOrEmpty(logMessage)) {
                _logger?.LogCritical(logMessage, logArgs);
            }

            if (IsService) {
                ServiceExit();
            } else {
                Environment.Exit(errorCode);
            }
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// #define WAIT_FOR_DEBUGGER

using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Broker.Logging;
using Microsoft.R.Host.Broker.Security;
using Microsoft.R.Host.Protocol;
using Newtonsoft.Json;

namespace Microsoft.R.Host.Broker.Start {
    public sealed class Configurator {
        public IConfigurationRoot Configuration { get; }
        public ILoggerFactory LoggerFactory { get; }
        public ILoggerProvider LoggerProvider => (ILoggerProvider)LoggerFactory;
        public StartupOptions StartupOptions { get; }
        public LoggingOptions LoggingOptions { get; }
        public bool IsService => StartupOptions?.IsService == true;
        public string Name => StartupOptions?.Name ?? "RTVS";
        public Uri Url { get; }

        public Configurator(string[] args) {
#if WAIT_FOR_DEBUGGER
            while (!System.Diagnostics.Debugger.IsAttached) {
                System.Threading.Thread.Sleep(1000);
            }
#endif

            Configuration = LoadConfiguration(LoggerFactory, args);
            StartupOptions = Configuration.GetStartupOptions();
            LoggingOptions = Configuration.GetLoggingOptions();

            LoggerFactory = new LoggerFactory2(StartupOptions, LoggingOptions)
                    .AddDebug()
                    .AddConsole(LogLevel.Trace);

            var s = Configuration.GetValue<string>(WebHostDefaults.ServerUrlsKey, null) ?? "https://0.0.0.0:5444";
            if (Uri.TryCreate(s, UriKind.Absolute, out var uri)) {
                Url = uri;
            }
        }

        public IWebHostBuilder ConfigureWebHost() {
            var builder = new WebHostBuilder()
                .ConfigureServices(s => s.AddSingleton(Configuration))
                .UseConfiguration(Configuration)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureLogging((c, logging) => {
                    logging
                        .AddConsole()
                        .AddDebug()
                        .AddProvider(LoggerProvider);
                });

            if (Url?.IsLoopback != true) {
                var httpsOptions = ConfigureHttps();
                builder.UseKestrel(options => options.Listen(IPAddress.Any, Url.Port, lo => lo.UseHttps(httpsOptions)));
            } else {
                builder.UseKestrel();
            }

            return builder;
        }

        private HttpsConnectionAdapterOptions ConfigureHttps() {
            var securityOptions = Configuration.GetSecuritySection().Get<SecurityOptions>();

            var logger = LoggerFactory.CreateLogger<TlsConfiguration>();
            var tlsConfig = new TlsConfiguration(logger, securityOptions);

            var httpsOptions = tlsConfig.GetHttpsOptions();
            if (httpsOptions == null) {

                logger.LogCritical(Resources.Critical_NoTlsCertificate, securityOptions.X509CertificateName);
                if (!IsService) {
                    Environment.Exit((int)BrokerExitCodes.NoCertificate);
                }
            }
            return httpsOptions;
        }

        private static IConfigurationRoot LoadConfiguration(ILoggerFactory loggerFactory, string[] args) {
            var configPath = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build()
                .GetValue<string>("config");

            var configuration = new ConfigurationBuilder().AddCommandLine(args);

            if (configPath != null) {
                try {
                    configuration.AddJsonFile(configPath, optional: false);
                } catch (IOException ex) {
                    loggerFactory.CreateLogger<Configurator>()
                        .LogCritical(Resources.Error_ConfigLoadFailed, ex.Message);
                    Environment.Exit((int)BrokerExitCodes.BadConfigFile);
                } catch (JsonException ex) {
                    loggerFactory.CreateLogger<Configurator>()
                        .LogCritical(Resources.Error_ConfigParseFailed, ex.Message);
                    Environment.Exit((int)BrokerExitCodes.BadConfigFile);
                }
            }

            return configuration.Build();
        }

        private class LoggerFactory2 : LoggerFactory, ILoggerProvider {
            public LoggerFactory2(StartupOptions startupOptions, LoggingOptions loggingOptions) {
                if (startupOptions != null && loggingOptions != null) {
                    var name = !string.IsNullOrEmpty(startupOptions.Name) ? startupOptions.Name : "RTVS";
                    var folder = loggingOptions.LogFolder;
                    this.AddFile(name, folder);
                }
            }
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.IO.Pipes;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Broker.Logging;
using Microsoft.R.Host.Broker.Security;
using Newtonsoft.Json;

namespace Microsoft.R.Host.Broker.Startup {
    public class Program {
        private static readonly ILoggerFactory _loggerFactory = new LoggerFactory();
        private static ILogger _logger;
        private static readonly StartupOptions _startupOptions = new StartupOptions();
        private static readonly SecurityOptions _securityOptions = new SecurityOptions();
        private static readonly CancellationTokenSource _cts = new CancellationTokenSource();

        internal static IConfigurationRoot Configuration { get; private set; }

        public static CancellationToken CancellationToken => _cts.Token;

        static Program() {
        }

        public static void Main(string[] args) {
            //MessageBox.Show("Broker");

            var configBuilder = new ConfigurationBuilder().AddCommandLine(args);
            Configuration = configBuilder.Build();

            string configFile = Configuration["config"];
            if (configFile != null) {
                configBuilder.AddJsonFile(configFile, optional: false);
                Configuration = configBuilder.Build();
            }

            ConfigurationBinder.Bind(Configuration.GetSection("startup"), _startupOptions);
            ConfigurationBinder.Bind(Configuration.GetSection("security"), _securityOptions);

            _loggerFactory
                .AddDebug()
                .AddConsole(LogLevel.Trace)
                .AddProvider(new FileLoggerProvider(_startupOptions.Name));
            _logger = _loggerFactory.CreateLogger<Program>();

            if (_startupOptions.Name != null) {
                _logger.LogInformation(Resources.Info_BrokerName, _startupOptions.Name);
            }

            var certificate = ConfigureTls();
            CreateWebHost(certificate).Run();
        }

        public static IWebHost CreateWebHost(X509Certificate2 certificate) {

            var webHostBuilder = new WebHostBuilder()
                .UseLoggerFactory(_loggerFactory)
                .UseConfiguration(Configuration)
                .UseKestrel(options => {
                    if (certificate != null) {
                        options.UseHttps(certificate);
                    }
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
                } catch (TimeoutException ex) {
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

        public static void Exit() {
            _cts.Cancel();

            Task.Run(async () => {
                // Give cooperative cancellation 10 seconds to shut the process down gracefully,
                // but if it didn't work, just terminate it.
                await Task.Delay(10000);
                _logger.LogCritical(Resources.Critical_TimeOutShutdown);
                Environment.Exit(1);
            });
        }

        private static X509Certificate2 ConfigureTls() {
            try {
                Uri uri;
                var url = Configuration.GetValue<string>("server.urls", null);
                if (Uri.TryCreate(url, UriKind.Absolute, out uri) && uri.IsLoopback) {
                    _logger.LogInformation(Resources.Trace_CertificateName, "none");
                    return null; // localhost, no TLS
                }
            } catch (Exception) { }

            X509Certificate2 certificate = null;
            certificate = Certificates.GetTLSCertificate();
            if (certificate == null) {
                _logger.LogCritical(Resources.Critical_NoTlsCertificate, _securityOptions.X509CertificateName);
                throw new InvalidOperationException(Resources.Critical_NoTlsCertificate);
            }
            _logger.LogInformation(Resources.Trace_CertificateName, certificate.FriendlyName);
            return certificate;
        }
    }
}

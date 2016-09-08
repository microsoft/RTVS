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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Broker.Logging;
using Newtonsoft.Json;

namespace Microsoft.R.Host.Broker.Startup {
    public class Program {
        private static readonly ILoggerFactory _loggerFactory = new LoggerFactory();
        private static ILogger _logger;
        private static readonly StartupOptions _startupOptions = new StartupOptions();
        private static readonly CancellationTokenSource _cts = new CancellationTokenSource();

        internal static IConfigurationRoot Configuration { get; private set; }

        public static CancellationToken CancellationToken => _cts.Token;

        static Program() {
        }

        public static void Main(string[] args) {
            var configBuilder = new ConfigurationBuilder().AddCommandLine(args);
            Configuration = configBuilder.Build();

            string configFile = Configuration["config"];
            if (configFile != null) {
                configBuilder.AddJsonFile(configFile, optional: false);
                Configuration = configBuilder.Build();
            }

            ConfigurationBinder.Bind(Configuration.GetSection("startup"), _startupOptions);

            _loggerFactory
                .AddDebug()
                .AddConsole(LogLevel.Trace)
                .AddProvider(new FileLoggerProvider(_startupOptions.Name));
            _logger = _loggerFactory.CreateLogger<Program>();

            if (_startupOptions.Name != null) {
                _logger.LogInformation($"Broker name '{_startupOptions.Name}' assigned");
            }

            CreateWebHost().Run();
        }

        public static IWebHost CreateWebHost() {
            var webHostBuilder = new WebHostBuilder()
                .UseLoggerFactory(_loggerFactory)
                .UseConfiguration(Configuration)
                .UseKestrel(options => {
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
                    _logger.LogCritical(0, ex, $"Requested to write server.urls to pipe '{pipeName}', but it is not a valid pipe handle.");
                    throw;
                } catch (TimeoutException ex) {
                    _logger.LogCritical(0, ex, $"Requested to write server.urls to pipe '{pipeName}', but timed out while trying to connect to pipe.");
                    throw;
                }

                var applicationLifetime = webHost.Services.GetService<IApplicationLifetime>();
                applicationLifetime.ApplicationStarted.Register(() => Task.Run(() => {
                    using (pipe) {
                        string serverUriStr = JsonConvert.SerializeObject(serverAddresses.Addresses);
                        _logger.LogTrace($"Writing server.urls to pipe '{pipeName}':{Environment.NewLine}{serverUriStr}");

                        var serverUriData = Encoding.UTF8.GetBytes(serverUriStr);
                        pipe.Write(serverUriData, 0, serverUriData.Length);
                        pipe.Flush();
                    }

                    _logger.LogTrace($"Wrote server.urls to pipe '{pipeName}'.");
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
                _logger.LogCritical("Timed out waiting for graceful shutdown");
                Environment.Exit(1);
            });
        }
    }
}

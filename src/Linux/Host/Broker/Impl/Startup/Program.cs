// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Broker.Logging;

namespace Microsoft.R.Host.Broker.Startup {
    public class Program {
        public static void Main(string[] args) {
            var configuration = new ConfigurationBuilder()
                .AddCommandLine(args)
                .Build();

            var loggerFactory = new LoggerFactory()
                    .AddDebug()
                    .AddConsole(LogLevel.Trace);

            configuration = Startup.LoadConfiguration(loggerFactory, configuration.GetValue<string>("config"), args);
            var startupOptions = configuration.GetStartupOptions();
            var loggingOptions = configuration.GetLoggingOptions();

            if (startupOptions != null && loggingOptions != null) {
                loggerFactory.AddFile(startupOptions.Name, loggingOptions.LogFolder);
            }

            var webHost = new WebHostBuilder()
                .ConfigureServices(s => s.AddSingleton(configuration))
                .UseLoggerFactory(loggerFactory)
                .UseConfiguration(configuration)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel()
                .UseStartup<LinuxStartup>()
                .Build();

            try {
                webHost.Run();
            } catch (Exception ex) {
                ex.HandleWebHostStartExceptions(webHost.Services.GetService<IServiceProvider>(), true);
            }
        }
    }
}
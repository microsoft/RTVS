// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ServiceProcess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;
using Microsoft.R.Host.Broker.Logging;

namespace Microsoft.R.Host.Broker.Startup {
    public class Program {
        public static void Main(string[] args) {
            var cm = new Configurator(args);

            if (cm.IsService) {
                cm.LoggerFactory.AddEventLog(new EventLogSettings {
                    Filter = (_, logLevel) => logLevel >= LogLevel.Warning,
                    SourceName = Resources.Text_ServiceName
                });
            }

            var logFolder = cm.LoggingOptions != null ? cm.LoggingOptions.LogFolder : Environment.GetEnvironmentVariable("TEMP");
            cm.LoggerFactory.AddFile(cm.Name, logFolder);

            var webHost = cm.Configure().Build();

            if (cm.IsService) {
                ServiceBase.Run(new BrokerService(webHost));
            } else {
                try {
                    webHost.Run();
                } catch (Exception ex) {
                    ex.HandleWebHostStartExceptions(webHost.Services.GetService<IServiceProvider>(), true);
                }
            }
        }
    }
}

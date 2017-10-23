// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.R.Host.Broker.Logging;

namespace Microsoft.R.Host.Broker.Startup {
    public class Program {
        public static void Main(string[] args) {
            var cm = new Configurator(args);

            if (cm.LoggingOptions != null) {
                cm.LoggerFactory.AddFile(cm.Name, cm.LoggingOptions.LogFolder);
            }

            var webHost = cm
                .Configure()
                .UseStartup<UnixStartup>()
                .Build();

            try {
                webHost.Run();
            } catch (Exception ex) {
                ex.HandleWebHostStartExceptions(webHost.Services.GetService<IServiceProvider>(), true);
            }
        }
    }
}
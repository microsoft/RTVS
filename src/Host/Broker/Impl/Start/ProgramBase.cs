// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

// #define WAIT_FOR_DEBUGGER

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.R.Host.Broker.Logging;

namespace Microsoft.R.Host.Broker.Start {
    public abstract class ProgramBase {
        public static IWebHost WebHost { get; protected set; }
        protected static void MainEntry<T>(string[] args) where T : Startup {
#if WAIT_FOR_DEBUGGER
            while (!System.Diagnostics.Debugger.IsAttached) {
                System.Threading.Thread.Sleep(1000);
            }
#endif
            var cm = new Configurator(args);

            if (cm.LoggingOptions != null) {
                cm.LoggerFactory.AddFile(cm.Name, cm.LoggingOptions.LogFolder);
            }

            WebHost = cm
                .ConfigureWebHost()
                .UseStartup<T>()
                .Build();

            try {
                WebHost.Run();
            } catch (Exception ex) {
                ex.HandleWebHostStartExceptions(WebHost.Services.GetService<IServiceProvider>(), true);
            }
        }
    }
}

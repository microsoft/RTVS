// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.R.Host.Broker.Logging;

namespace Microsoft.R.Host.Broker.Start {
    public abstract class ProgramBase {
        public static IWebHost WebHost { get; protected set; }
        protected static void MainEntry<T>(string[] args) where T : Startup {
            var cm = new Configurator(args);

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

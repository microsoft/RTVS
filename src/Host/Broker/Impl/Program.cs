// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Server;

namespace Microsoft.R.Host.Broker {
    public class Program {
        internal static IConfigurationRoot Configuration { get; private set; }

        public static void Main(string[] args) {
            var configBuilder = new ConfigurationBuilder().AddCommandLine(args);
            Configuration = configBuilder.Build();

            string configFile = Configuration["configFile"];
            if (configFile != null) {
                configBuilder.AddJsonFile(configFile, optional: false);
                Configuration = configBuilder.Build();
            }

            var host = new WebHostBuilder()
                .UseConfiguration(Configuration)
                .UseWebListener(options => options.Listener.AuthenticationManager.AuthenticationSchemes = AuthenticationSchemes.NTLM)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}

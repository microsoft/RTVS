// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Common.Core;

namespace Microsoft.R.Host.Broker.Startup {
    public class Program {
        static Program() { }

        public static void Main(string[] args) {
            var configBuilder = new ConfigurationBuilder().AddCommandLine(args);
            var configuration = configBuilder.Build();

            string startAs = configuration["start.as"];
            string configFile = configuration["config"];
            if (configFile != null) {
                configBuilder.AddJsonFile(configFile, optional: false);
                configuration = configBuilder.Build();
            }

            CommonStartup.CommonStartupInit(configuration);
            if (string.IsNullOrWhiteSpace(startAs) || startAs.EqualsIgnoreCase("app")) {
                CommonStartup.StartApp(configuration);
            } else {
                CommonStartup.StartService(configuration);
            }
        }
    }
}

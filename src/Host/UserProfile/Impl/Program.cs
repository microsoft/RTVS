// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ServiceProcess;

namespace Microsoft.R.Host.UserProfile {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main() {
#if DEBUG
            // Uncomment the line below to debug the Broker-Service
            // Debugger.Launch();
#endif
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new RUserProfileService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}

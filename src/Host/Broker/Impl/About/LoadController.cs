// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Management;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.R.Host.Broker.Security;
using Microsoft.R.Host.Broker.Sessions;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Broker.About {
    [Authorize(Policy = Policies.RUser)]
    [Route("/HostLoad")]
    public class LoadController : Controller {
        private readonly PerformanceCounter _cpuCounter = new PerformanceCounter("Performance", "CPU");
        private readonly PerformanceCounter _networkCounter = new PerformanceCounter("Performance", "Network");

        public LoadController(SessionManager sessionManager) {
        }

        [HttpGet]
        public HostLoad Get() {
            var h = new HostLoad();

            var query = new SelectQuery(@"Select * from Win32_OperatingSystem");
            using (var search = new ManagementObjectSearcher(query)) {
                foreach (var mo in search.Get()) {
                    var totalMemory = GetSizeInGB(mo, "TotalVisibleMemorySize");
                    var freeMemory = GetSizeInGB(mo, "FreePhysicalMemory");
                    h.MemoryLoad = freeMemory / totalMemory;
                    break;
                }
            }

            h.CpuLoad = _cpuCounter.NextValue();
            h.NetworkLoad = _networkCounter.NextValue();

            return h;
        }

        private long GetSizeInGB(ManagementBaseObject mo, string name) {
            int result;
            var x = mo[name].ToString();
            return Int32.TryParse(x, out result) ? result / 1024 : 0;
        }
    }
}

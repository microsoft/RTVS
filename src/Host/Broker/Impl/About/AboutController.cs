// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Management;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.R.Host.Broker.Interpreters;
using Microsoft.R.Host.Broker.Security;
using Microsoft.R.Host.Protocol;
using Microsoft.R.Interpreters;

namespace Microsoft.R.Host.Broker.About {
    [Authorize(Policy = Policies.RUser)]
    [Route("/about")]
    public class AboutController : Controller {
        private readonly InterpreterManager _interpManager;

        public AboutController(InterpreterManager interpManager) {
            _interpManager = interpManager;
        }

        [HttpGet]
        public AboutHost Get() {
            var a = new AboutHost();

            a.Version = Assembly.GetExecutingAssembly().GetName().Version;
            a.OS = Environment.OSVersion;
            a.Is64BitOperatingSystem = Environment.Is64BitOperatingSystem;
            a.Is64BitProcess = Environment.Is64BitProcess;
            a.ProcessorCount = Environment.ProcessorCount;
            a.WorkingSet = Environment.WorkingSet;

            var query = new SelectQuery(@"Select * from Win32_OperatingSystem");
            using (var search = new ManagementObjectSearcher(query)) {
                foreach (var mo in search.Get()) {
                    a.TotalVirtualMemory = GetValue(mo, "TotalVirtualMemorySize");
                    a.FreeVirtualMemory = GetValue(mo, "FreeVirtualMemory");
                    a.TotalPhysicalMemory = GetValue(mo, "TotalVisibleMemorySize");
                    a.FreePhysicalMemory = GetValue(mo, "FreePhysicalMemory");
                    break;
                }
            }

            a.Interpreters = new RInstallation().GetCompatibleEngines().Select(e => e.Name).ToArray();
            return a;
        }

        private long GetValue(ManagementBaseObject mo, string name) {
            int result;
            var x = mo[name].ToString();
            return Int32.TryParse(x, out result) ? result / 1024 : 0;
        }
    }
}

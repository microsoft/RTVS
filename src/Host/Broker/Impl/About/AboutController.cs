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
using Microsoft.R.Host.Broker.Sessions;
using Microsoft.R.Host.Protocol;
using static System.FormattableString;

namespace Microsoft.R.Host.Broker.About {
    [Authorize(Policy = Policies.RUser)]
    [Route("/info/about")]
    public class AboutController : Controller {
        private readonly InterpreterManager _interpManager;
        private readonly SessionManager _sessionManager;

        public AboutController(InterpreterManager interpManager, SessionManager sessionManager) {
            _interpManager = interpManager;
            _sessionManager = sessionManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public AboutHost Get() {
            var a = new AboutHost();

            a.Version = Assembly.GetExecutingAssembly().GetName().Version;
            a.OS = Environment.OSVersion;
            a.Is64BitOperatingSystem = Environment.Is64BitOperatingSystem;
            a.Is64BitProcess = Environment.Is64BitProcess;
            a.ProcessorCount = Environment.ProcessorCount;
            a.WorkingSet = Environment.WorkingSet;
            a.ConnectedUserCount = _sessionManager.GetUsers().Count();

            GetMemoryInformation(ref a);
            GetVideoControllerInformation(ref a);

            a.Interpreters = _interpManager.Interpreters.Select(x => Invariant($"[{x.Id}] {x.Name}")).ToArray();
            return a;
        }

        private long ToLong(ManagementBaseObject mo, string name, int divider) {
            int result;
            var value = mo[name];
            if (value != null) {
                var x = value.ToString();
                return Int32.TryParse(x, out result) ? result / divider : 0;
            }
            return 0;
        }

        private void GetMemoryInformation(ref AboutHost a) {
            using (var search = new ManagementObjectSearcher("Select * from Win32_OperatingSystem")) {
                foreach (var mo in search.Get()) {
                    a.TotalVirtualMemory = ToLong(mo, "TotalVirtualMemorySize", 1024);
                    a.FreeVirtualMemory = ToLong(mo, "FreeVirtualMemory", 1024);
                    a.TotalPhysicalMemory = ToLong(mo, "TotalVisibleMemorySize", 1024);
                    a.FreePhysicalMemory = ToLong(mo, "FreePhysicalMemory", 1024);
                    break;
                }
            }
        }

        private void GetVideoControllerInformation(ref AboutHost a) {
            using (var search = new ManagementObjectSearcher("Select * from Win32_VideoController")) {
                foreach (var mo in search.Get()) {
                    a.VideoCardName = mo["Name"]?.ToString();
                    a.VideoRAM = ToLong(mo, "AdapterRAM", 1024 * 1024);
                    a.VideoProcessor = mo["VideoProcessor"]?.ToString();
                }
            }
        }
    }
}

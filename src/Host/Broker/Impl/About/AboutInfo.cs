// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Management;
using System.Reflection;
using Microsoft.R.Host.Broker.Interpreters;
using Microsoft.R.Host.Broker.Sessions;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Broker.About {
    public class AboutInfo {
        private static InterpreterManager _interpManager;
        private static SessionManager _sessionManager;

        public static void Initialize(InterpreterManager interpManager, SessionManager sessionManager) {
            _interpManager = interpManager;
            _sessionManager = sessionManager;
        }

        public static AboutHost GetAboutHost() {
            var a = new AboutHost();

            a.Version = Assembly.GetExecutingAssembly().GetName().Version;
            a.OS = Environment.OSVersion;
            a.Is64BitOperatingSystem = Environment.Is64BitOperatingSystem;
            a.Is64BitProcess = Environment.Is64BitProcess;
            a.ProcessorCount = Environment.ProcessorCount;
            a.WorkingSet = Environment.WorkingSet;
            a.ConnectedUserCount = _sessionManager.GetUsers().Count();

            var query = new SelectQuery(@"Select * from Win32_OperatingSystem");
            using (var search = new ManagementObjectSearcher(query)) {
                foreach (var mo in search.Get()) {
                    a.TotalVirtualMemory = GetSizeInGB(mo, "TotalVirtualMemorySize");
                    a.FreeVirtualMemory = GetSizeInGB(mo, "FreeVirtualMemory");
                    a.TotalPhysicalMemory = GetSizeInGB(mo, "TotalVisibleMemorySize");
                    a.FreePhysicalMemory = GetSizeInGB(mo, "FreePhysicalMemory");
                    break;
                }
            }

            a.Interpreters = _interpManager.Interpreters.Select(x => x.Name).ToArray();
            return a;
        }

        private static long GetSizeInGB(ManagementBaseObject mo, string name) {
            int result;
            var x = mo[name].ToString();
            return Int32.TryParse(x, out result) ? result / 1024 : 0;
        }
    }
}

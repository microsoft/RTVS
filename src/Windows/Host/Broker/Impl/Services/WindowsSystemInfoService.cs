// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.Management;
using System.Threading;

namespace Microsoft.R.Host.Broker.Services {
    public class WindowsSystemInfoService : ISystemInfoService {
        // https://msdn.microsoft.com/en-us/library/2fh4x1xb(v=vs.100).aspx
        private readonly PerformanceCounter _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        public WindowsSystemInfoService() {
            _cpuCounter.NextValue(); // first call always returns 0
        }

        public double GetCpuLoad() {
            double counter = 0;
            int iterations = 5;

            for (int i = 0; i < iterations; i++) {
                Thread.Sleep(200);
                counter += _cpuCounter.NextValue();
            }

            return counter / (iterations * 100);
        }

        public (long TotalVirtualMemory, long FreeVirtualMemory, long TotalPhysicalMemory, long FreePhysicalMemory) GetMemoryInformation() {
            using (var search = new ManagementObjectSearcher("Select * from Win32_OperatingSystem")) {
                foreach (var mo in search.Get()) {
                    return (
                        ToLong(mo, "TotalVirtualMemorySize", 1024),
                        ToLong(mo, "FreeVirtualMemory", 1024),
                        ToLong(mo, "TotalVisibleMemorySize", 1024),
                        ToLong(mo, "FreePhysicalMemory", 1024)
                    );
                }
            }

            return (0, 0, 0, 0);
        }

        public (string VideoCardName, long VideoRAM, string VideoProcessor) GetVideoControllerInformation() {
            using (var search = new ManagementObjectSearcher("Select * from Win32_VideoController")) {
                foreach (var mo in search.Get()) {
                    return (
                        mo["Name"]?.ToString(), 
                        ToLong(mo, "AdapterRAM", 1024 * 1024), 
                        mo["VideoProcessor"]?.ToString()
                    );
                }
            }

            return (string.Empty, 0, string.Empty);
        }
        
        private static long ToLong(ManagementBaseObject mo, string name, int divider) {
            var value = mo[name];
            return value == null 
                ? 0
                : (int.TryParse(value.ToString(), out int result) ? result / divider : 0);
        }
    }
}
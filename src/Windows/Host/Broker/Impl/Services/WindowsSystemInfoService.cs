// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Threading;
using Microsoft.R.Host.Protocol;

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

        public double GetNetworkLoad() {
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return 0;
            }

            // Select compatible active network interfaces
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(x => x.Speed > 0 
                    && x.Supports(NetworkInterfaceComponent.IPv4) 
                    && x.Supports(NetworkInterfaceComponent.IPv6) 
                    && x.OperationalStatus == OperationalStatus.Up 
                    && IsCompatibleInterface(x.NetworkInterfaceType)).ToArray();

            // Take initial measurement, wait 500ms, take another.
            var stats = interfaces.Select(s => s.GetIPStatistics()).ToArray();
            var initialBytes = stats.Select(s => s.BytesSent + s.BytesReceived).ToArray();

            Thread.Sleep(500);

            stats = interfaces.Select(s => s.GetIPStatistics()).ToArray();
            var currentBytes = stats.Select(s => s.BytesSent + s.BytesReceived).ToArray();

            // Figure out how many bytes were sent and received within 500ms
            // and calculate adapter load depending on speed. Take the highest value.
            double maxLoad = 0;
            for (int i = 0; i < initialBytes.Length; i++) {
                // 16 = 8 bits per byte in 1/2 second. Speed it in bits per second.
                var load = 16.0 * (currentBytes[i] - initialBytes[i]) / interfaces[i].Speed;
                maxLoad = Math.Max(maxLoad, load);
            }

            return maxLoad;
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

        public IEnumerable<VideoCardInfo> GetVideoControllerInformation() {
            using (var search = new ManagementObjectSearcher("Select * from Win32_VideoController")) {
                foreach (var mo in search.Get()) {
                    yield return new VideoCardInfo() {
                        VideoCardName = mo["Name"]?.ToString(),
                        VideoRAM = ToLong(mo, "AdapterRAM", 1024 * 1024),
                        VideoProcessor = mo["VideoProcessor"]?.ToString()
                    };
                }
            }

            yield return new VideoCardInfo();
        }
        
        private static long ToLong(ManagementBaseObject mo, string name, int divider) {
            var value = mo[name];
            return value == null 
                ? 0
                : (int.TryParse(value.ToString(), out int result) ? result / divider : 0);
        }

        private static bool IsCompatibleInterface(NetworkInterfaceType nit) {
            switch (nit) {
                case NetworkInterfaceType.Loopback:
                case NetworkInterfaceType.HighPerformanceSerialBus:
                case NetworkInterfaceType.Ppp:
                    return false;
                default:
                    return true;
            }
        }
    }
}
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Threading;
using System.Timers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Common.Core;
using Microsoft.R.Host.Broker.Security;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Broker.About {
    [Authorize(Policy = Policies.RUser)]
    [Route("/info/load")]
    public class LoadController : Controller {
        // https://msdn.microsoft.com/en-us/library/2fh4x1xb(v=vs.100).aspx
        private readonly PerformanceCounter _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private readonly System.Timers.Timer _timer = new System.Timers.Timer();
        private readonly HostLoad _hostLoad = new HostLoad();

        public LoadController() {
            _timer.Interval = 3000;
            _timer.AutoReset = true;
            _timer.Elapsed += OnTimer;

            _cpuCounter.NextValue(); // first call always returns 0
            UpdateMeasurement();
        }

        [HttpGet]
        public HostLoad Get() {
            return _hostLoad;
        }

        private void UpdateMeasurement() {
            var query = new SelectQuery(@"Select * from Win32_OperatingSystem");
            using (var search = new ManagementObjectSearcher(query)) {
                foreach (var mo in search.Get()) {
                    var totalMemory = GetSizeInGB(mo, "TotalVisibleMemorySize");
                    var freeMemory = GetSizeInGB(mo, "FreePhysicalMemory");
                    _hostLoad.MemoryLoad = (double)(totalMemory - freeMemory) / (double)totalMemory;
                    break;
                }
            }

            _hostLoad.CpuLoad = GetCpuLoad();
            _hostLoad.NetworkLoad = GetNetworkLoad();
        }

        protected override void Dispose(bool disposing) {
            _timer?.Stop();
            _timer?.Dispose();
            base.Dispose(disposing);
        }

        private void OnTimer(object sender, ElapsedEventArgs e) {
            UpdateMeasurement();
        }

        private long GetSizeInGB(ManagementBaseObject mo, string name) {
            int result;
            var x = mo[name].ToString();
            return Int32.TryParse(x, out result) ? result / 1024 : 0;
        }

        private double GetCpuLoad() {
            double counter = 0;
            int iterations = 5;

            for (int i = 0; i < iterations; i++) {
                Thread.Sleep(200);
                counter += _cpuCounter.NextValue();
            }

            return counter / (iterations * 100);
        }

        private double GetNetworkLoad() {
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return 0;
            }

            // Select compatible active network interfaces
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                                             .Where(x => x.Speed > 0 &&
                                                         x.Supports(NetworkInterfaceComponent.IPv4) &&
                                                         x.Supports(NetworkInterfaceComponent.IPv6) &&
                                                         x.OperationalStatus == OperationalStatus.Up &&
                                                         x.Speed > 0 &&
                                                         IsCompatibleInterface(x.NetworkInterfaceType)).ToArray();

            // Take initial measurement, wait 500ms, take another.
            var stats = interfaces.Select(s => s.GetIPStatistics()).ToArray();
            var initialBytes = stats.Select(s => s.BytesSent + s.BytesReceived).ToArray();

            Thread.Sleep(500);

            stats = interfaces.Select(s => s.GetIPStatistics()).ToArray();
            var currentBytes = stats.Select(s => s.BytesSent + s.BytesReceived).ToArray();

            // Figure out how many bytes were sent and received within 500ms
            // and calculate adapter load depending on speed. Take the highest value.
            double load = 0;
            double maxLoad = 0;
            for (int i = 0; i < initialBytes.Length; i++) {
                // 16 = 8 bits per byte in 1/2 second. Speed it in bits per second.
                load = 16.0 * (currentBytes[i] - initialBytes[i]) / (double)interfaces[i].Speed;
                maxLoad = Math.Max(maxLoad, load);
            }

            return maxLoad;
        }

        private static bool IsCompatibleInterface(NetworkInterfaceType nit) {
            switch (nit) {
                case NetworkInterfaceType.Loopback:
                case NetworkInterfaceType.HighPerformanceSerialBus:
                case NetworkInterfaceType.Ppp:
                    return false;
            }
            return true;
        }
    }
}
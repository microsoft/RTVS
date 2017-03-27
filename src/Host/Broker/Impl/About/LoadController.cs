// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.R.Host.Broker.Security;
using Microsoft.R.Host.Broker.Services;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Broker.About {
    [Authorize(Policy = Policies.RUser)]
    [Route("/info/load")]
    public class LoadController : Controller {
        private readonly Timer _timer;
        private readonly HostLoad _hostLoad = new HostLoad();
        private readonly ISystemInfoService _systemInfo;

        public LoadController(ISystemInfoService systemInfo) {
            _systemInfo = systemInfo;
            _timer = new Timer(UpdateMeasurement, null, 0, 3000);
            UpdateMeasurement(null);
        }

        [AllowAnonymous]
        [HttpGet]
        public HostLoad Get() {
            return _hostLoad;
        }

        private void UpdateMeasurement(object state) {
            var memoryInfo = _systemInfo.GetMemoryInformation();
            _hostLoad.MemoryLoad = (double)(memoryInfo.TotalPhysicalMemory - memoryInfo.FreePhysicalMemory) / memoryInfo.TotalPhysicalMemory;
            _hostLoad.CpuLoad = _systemInfo.GetCpuLoad();
            _hostLoad.NetworkLoad = GetNetworkLoad();
        }

        protected override void Dispose(bool disposing) {
            _timer?.Dispose();
            base.Dispose(disposing);
        }

        private double GetNetworkLoad() {
            if (!NetworkInterface.GetIsNetworkAvailable()) {
                return 0;
            }

            // Select compatible active network interfaces
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(x => x.Speed > 0 
                    && x.Supports(NetworkInterfaceComponent.IPv4) 
                    && x.Supports(NetworkInterfaceComponent.IPv6) 
                    && x.OperationalStatus == OperationalStatus.Up 
                    && x.Speed > 0 && IsCompatibleInterface(x.NetworkInterfaceType)).ToArray();

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
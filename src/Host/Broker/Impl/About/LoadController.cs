// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading;
using System.Timers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Common.Core;
using Microsoft.R.Host.Broker.Security;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Broker.About {
    [Authorize(Policy = Policies.RUser)]
    [Route("/HostLoad")]
    public class LoadController : Controller {
        // https://msdn.microsoft.com/en-us/library/2fh4x1xb(v=vs.100).aspx
        private readonly PerformanceCounter _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        private readonly string _networkInstanceName;
        private readonly PerformanceCounter _networkSentCounter;
        private readonly PerformanceCounter _networkReceivedCounter;
        private readonly double _networkBandwidth;
        private readonly System.Timers.Timer _timer = new System.Timers.Timer();
        private readonly HostLoad _hostLoad = new HostLoad();

        public LoadController() {
            var category = "Network Interface";
            _networkInstanceName = GetCategoryInstanceName(category, (x) => x.StartsWithOrdinal("Local"));
            _networkBandwidth = (new PerformanceCounter(category, "Current Bandwidth", _networkInstanceName)).NextValue();
            _networkSentCounter = new PerformanceCounter(category, "Bytes Sent/sec", _networkInstanceName);
            _networkReceivedCounter = new PerformanceCounter(category, "Bytes Received/sec", _networkInstanceName);

            _timer.Interval = 2000;
            _timer.AutoReset = true;
            _timer.Elapsed += OnTimer;

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

        private static string GetCategoryInstanceName(string category, Func<string, bool> filter) {
            var cat = new PerformanceCounterCategory(category);
            return cat.GetInstanceNames().Where(c => filter(c)).FirstOrDefault();
        }

        private double GetCpuLoad() {
            double counter = 0;
            int iterations = 10;

            for (int i = 0; i < iterations; i++) {
                Thread.Sleep(10);
                counter += _cpuCounter.NextValue();
            }

            return counter / (iterations * 100);
        }

        private double GetNetworkLoad() {
            double sent = 0;
            double received = 0;
            int iterations = 10;

            for (int i = 0; i < iterations; i++) {
                Thread.Sleep(10);
                sent += _networkSentCounter.NextValue();
                received += _networkReceivedCounter.NextValue();
            }

            return (8 * (sent + received)) / (_networkBandwidth * iterations);
        }

        //public static List<string> GetNames(string category) {
        //    var cat = new PerformanceCounterCategory(category);
        //    var list = new List<string>();
        //    try {
        //        var names = cat.GetInstanceNames();
        //        if (names.Length == 0) {
        //            list.AddRange(cat.GetCounters().Select(x => x.CounterName));
        //        }
        //        foreach (var n in names) {
        //            list.AddRange(cat.GetCounters(n).Select(x => x.CounterName));
        //        }
        //    } catch (Exception) { }

        //    return list;
        //}
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

namespace Microsoft.R.Host.Broker.Services {
    public class LinuxSystemInfoService : ISystemInfoService {
        private readonly IFileSystem _fs;
        private static char[] _memInfoSplitter = new char[] { ' ', ':', '\t' };
        private static Dictionary<string, long> _sizeLUT = new Dictionary<string, long> {
            {"KB", 1000L},
            {"MB", 1000L * 1000L},
            {"GB", 1000L * 1000L * 1000L},
            {"TB", 1000L * 1000L * 1000L * 1000L},
        };

        public LinuxSystemInfoService(IFileSystem fs) {
            _fs = fs;
        }

        public double GetCpuLoad() {
            var initial = GetCurrentCpuUsage();
            Thread.Sleep(500);
            var current = GetCurrentCpuUsage();

            long initTimeSinceBoot = initial.user + initial.nice + initial.system + initial.idle + initial.iowait + initial.irq + initial.softirq + initial.steal;
            long currentTimeSinceBoot = current.user + current.nice + current.system + current.idle + current.iowait + current.irq + current.softirq + current.steal;
            long deltaTimeSinceBoot = currentTimeSinceBoot - initTimeSinceBoot;

            long initIdleTime = initial.idle + initial.iowait;
            long currentIdleTime = current.idle + current.iowait;
            long deltaIdleTime = currentIdleTime - initIdleTime;

            long deltaUsage = deltaTimeSinceBoot - deltaIdleTime;
            return deltaUsage / deltaTimeSinceBoot;
        }

        public (long TotalVirtualMemory, long FreeVirtualMemory, long TotalPhysicalMemory, long FreePhysicalMemory) GetMemoryInformation() {
            try {
                string memInfoFile = "/proc/meminfo";
                var data = _fs.FileReadAllLines(memInfoFile);

                Dictionary<string, long> meminfo = new Dictionary<string, long>();
                foreach (string line in data) {
                    string[] split = line.Split(_memInfoSplitter, StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length == 3) {
                        long value;
                        if (split.Length > 2 && long.TryParse(split[1], out value)) {
                            long multiplier;
                            if (!_sizeLUT.TryGetValue(split[2].ToUpper(), out multiplier)) {
                                multiplier = 1;
                            }
                            meminfo.Add(split[0], value * multiplier);
                        } else if (long.TryParse(split[1], out value)) {
                            meminfo.Add(split[0], value);
                        }
                    }
                }
                return (
                    meminfo["SwapTotal"] / (1000 * 1000),
                    meminfo["SwapFree"] / (1000 * 1000),
                    meminfo["MemTotal"] / (1000 * 1000),
                    meminfo["MemFree"] / (1000 * 1000)
                    );
            } catch(Exception ex) when (!ex.IsCriticalException()) {
                return (1, 1, 1, 1);
            }
        }

        public double GetNetworkLoad() {
            try {
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
            } catch (PlatformNotSupportedException) {
                return 0;
            }
        }

        public (string VideoCardName, long VideoRAM, string VideoProcessor) GetVideoControllerInformation() {
            return ("", 0, "");
        }

        private static char[] _cpuInfoSplitter = new char[] { ' ', ':', '\t' };
        private (long user, long nice, long system, long idle, long iowait, long irq, long softirq, long steal) GetCurrentCpuUsage() {
            string cpuLoadInfo = "/proc/stat";
            var lines = _fs.FileReadAllLines(cpuLoadInfo).ToArray();
            if (lines.Length > 0) {
                var split = lines[0].Split(_cpuInfoSplitter, StringSplitOptions.RemoveEmptyEntries);
                long user, nice, system, idle, iowait, irq, softirq, steal;
                user = nice = system = idle = iowait = irq = softirq = steal = 0;
                // split[0] == "cpu" so ignore the first item;
                if (split.Length > 1) {
                    long.TryParse(split[1], out user);
                }

                if (split.Length > 2) {
                    long.TryParse(split[1], out nice);
                }

                if (split.Length > 3) {
                    long.TryParse(split[3], out system);
                }

                if (split.Length > 4) {
                    long.TryParse(split[4], out idle);
                }

                if (split.Length > 5) {
                    long.TryParse(split[5], out iowait);
                }

                if (split.Length > 6) {
                    long.TryParse(split[6], out irq);
                }

                if (split.Length > 7) {
                    long.TryParse(split[7], out softirq);
                }

                if (split.Length > 8) {
                    long.TryParse(split[8], out steal);
                }
                return (user, nice, system, idle, iowait, irq, softirq, steal);
            }
            return (0, 0, 0, 0, 0, 0, 0, 0);
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
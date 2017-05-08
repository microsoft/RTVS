// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.OS;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Broker.Services {
    public class LinuxSystemInfoService : ISystemInfoService {
        private readonly IFileSystem _fs;
        private readonly IProcessServices _ps;
        private static char[] _memInfoSplitter = new char[] { ' ', ':', '\t' };
        private static Dictionary<string, long> _sizeLUT = new Dictionary<string, long> {
            {"KB", 1000L},
            {"MB", 1000L * 1000L},
            {"GB", 1000L * 1000L * 1000L},
            {"TB", 1000L * 1000L * 1000L * 1000L},
        };

        public LinuxSystemInfoService(IFileSystem fs, IProcessServices ps) {
            _fs = fs;
            _ps = ps;
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


        private static object _videoCardInfoLock = new object();
        private static List<VideoCardInfo> _videoCardInfo;
        public IEnumerable<VideoCardInfo> GetVideoControllerInformation() {
            lock (_videoCardInfoLock) {
                if(_videoCardInfo != null) {
                    return _videoCardInfo;
                }

                InitializeVideoCardInfo();
                return _videoCardInfo;
            }
        }

        private void InitializeVideoCardInfo() {
            _videoCardInfo = new List<VideoCardInfo>();
            const string vramPart = "VRAM:";
            var lshwDisplayData = ExecuteAndGetOutput("/usr/bin/lshw", "-class display").Select(s => s.Trim()).ToArray();
            var dmesgData = ExecuteAndGetOutput("dmesg", null).Where(s => s.ContainsIgnoreCase(vramPart)).ToArray();

            string productPart = "product: ";
            string vendorPart = "vendor: ";
            string busPart = "bus info: ";

            // Match anything that looks like 1024M or 1024MB
            Regex pattern = new Regex("[0-9]+[GM][B]?", RegexOptions.IgnoreCase);

            int count = 1;
            for (int i=0;  i < lshwDisplayData.Length; ++i) {
                // find the line that starts with "*-" 
                if (lshwDisplayData[i].StartsWithIgnoreCase("*-")) {
                    string productName = GetPart(lshwDisplayData, productPart, ref i);
                    string vendorName = GetPart(lshwDisplayData, vendorPart, ref i);
                    string busInfo = GetPart(lshwDisplayData, busPart, ref i);
                    long vram = 0;
                    if (!string.IsNullOrEmpty(busInfo)) {
                        int index = busInfo.IndexOf('@');
                        if (index >= 0 && index < busInfo.Length) {
                            string busvalue = busInfo.Substring(index + 1);
                            for (int j = 0; j < dmesgData.Length; ++j) {
                                if (dmesgData[i].ContainsIgnoreCase(busvalue)) {
                                    var match = pattern.Match(dmesgData[i]);
                                    if (match.Success) {
                                        vram = GetRamValueMB(match.Value);
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    _videoCardInfo.Add(new VideoCardInfo() { VideoCardName = productName, VideoRAM = vram, VideoProcessor = vendorName });
                    ++count;
                }
            }
        }

        private static long GetRamValueMB(string vramStr) {
            long ram = 0;
            Regex numPattern = new Regex("[0-9]+");
            var match = numPattern.Match(vramStr);
            
            if (match.Success) {
                long.TryParse(match.Value, out ram);
            }

            if (vramStr.ContainsIgnoreCase("G")) {
                ram *= 1024;
            }

            return ram;
        }

        private IEnumerable<string> ExecuteAndGetOutput(string command, string arguments) {
            Process proc = null;
            List<string> standardOutData = new List<string>();
            try {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.Arguments = arguments;
                psi.FileName = command;
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                proc = _ps.Start(psi);

                using (StreamReader reader = new StreamReader(proc.StandardOutput.BaseStream)) {
                    while (!reader.EndOfStream) {
                        standardOutData.Add(reader.ReadLine());
                    }
                }
            } catch (Exception) {
            } finally {
                if (proc != null && !proc.HasExited) {
                    proc?.Kill();
                }
            }
            return standardOutData;
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

        private static string GetPart(string[] lines, string part, ref int index) {
            if (index < lines.Length) {
                string line = lines[index];
                while (!line.StartsWithOrdinal(part)) {
                    if (++index >= lines.Length) {
                        return null;
                    }
                    line = lines[index];
                }
                return line.Substring(part.Length).Trim();
            }
            return null;
        }
    }
}
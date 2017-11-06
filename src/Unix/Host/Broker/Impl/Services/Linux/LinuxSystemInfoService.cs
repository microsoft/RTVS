// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.OS;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Protocol;

namespace Microsoft.R.Host.Broker.Services.Linux {
    public class LinuxSystemInfoService : ISystemInfoService {
        private readonly ILogger<ISystemInfoService> _logger;
        private readonly IFileSystem _fs;
        private readonly IProcessServices _ps;
        private readonly static char[] _memInfoSplitter = new char[] { ' ', ':', '\t' };
        private readonly static Dictionary<string, long> _sizeLUT = new Dictionary<string, long> {
            {"KB", 1000L},
            {"MB", 1000L * 1000L},
            {"GB", 1000L * 1000L * 1000L},
            {"TB", 1000L * 1000L * 1000L * 1000L},
        };

        // Match anything that looks like 1024M or 1024MB
        private readonly static Regex _ramPattern = new Regex(@"[\d]+[GM][B]?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Match numbrs in a string
        private readonly static Regex _numPattern = new Regex(@"[\d]+", RegexOptions.Compiled);

        // Match 'whitespace' at the begining of a string
        private readonly static Regex _indentPattern = new Regex(@"(?<indent>[\s]*).*", RegexOptions.Compiled);

        // Match '<whitespace><key>:<value>' in a string
        private readonly static Regex _keyValuePairsPattern = new Regex(@"[\s]*(?<key>[ \w]*):(?<value>.*)", RegexOptions.Compiled);

        public LinuxSystemInfoService(ILogger<ISystemInfoService> logger, IFileSystem fs, IProcessServices ps) {
            _logger = logger;
            _fs = fs;
            _ps = ps;
        }

        public double GetCpuLoad() {
            var initial = GetCurrentCpuUsage();
            Thread.Sleep(500);
            var current = GetCurrentCpuUsage();

            var initTimeSinceBoot = initial.user + initial.nice + initial.system + initial.idle + initial.iowait + initial.irq + initial.softirq + initial.steal;
            var currentTimeSinceBoot = current.user + current.nice + current.system + current.idle + current.iowait + current.irq + current.softirq + current.steal;
            var deltaTimeSinceBoot = currentTimeSinceBoot - initTimeSinceBoot;

            var initIdleTime = initial.idle + initial.iowait;
            var currentIdleTime = current.idle + current.iowait;
            var deltaIdleTime = currentIdleTime - initIdleTime;

            var deltaUsage = deltaTimeSinceBoot - deltaIdleTime;
            return deltaUsage / deltaTimeSinceBoot;
        }

        public (long TotalVirtualMemory, long FreeVirtualMemory, long TotalPhysicalMemory, long FreePhysicalMemory) GetMemoryInformation() {
            try {
                var memInfoFile = "/proc/meminfo";
                var data = _fs.FileReadAllLines(memInfoFile);

                var meminfo = new Dictionary<string, long>();
                foreach (var line in data) {
                    var split = line.Split(_memInfoSplitter, StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length == 3) {
                        if (split.Length > 2 && long.TryParse(split[1], out var value)) {
                            if (!_sizeLUT.TryGetValue(split[2].ToUpper(), out var multiplier)) {
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
            if (RuntimeInformation.OSDescription.Contains("Microsoft")) {
                // We are on Windows Subsystem for Linux. Return 0 here due to bug:
                // https://github.com/dotnet/corefx/issues/22048
                return 0;
            }

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
                for (var i = 0; i < initialBytes.Length; i++) {
                    // 16 = 8 bits per byte in 1/2 second. Speed it in bits per second.
                    var load = 16.0 * (currentBytes[i] - initialBytes[i]) / interfaces[i].Speed;
                    maxLoad = Math.Max(maxLoad, load);
                }
                return maxLoad;
            } catch (Exception ex) when (!ex.IsCriticalException()) {
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
            var lshwDisplayData = ParseDisplayData(ExecuteAndGetOutput("/usr/bin/lshw", "-c display"));
            var dmesgData = ExecuteAndGetOutput("dmesg", null).Where(s => s.ContainsIgnoreCase(vramPart)).ToArray();

            foreach (var displayData in lshwDisplayData) {
                displayData.TryGetValue("product", out var productName);
                displayData.TryGetValue("vendor", out var vendorName);

                long vram = 0;
                if (displayData.TryGetValue("bus info", out var busInfo)) {
                    var index = busInfo.IndexOf('@');
                    if (index >= 0 && index < busInfo.Length) {
                        var busvalue = busInfo.Substring(index + 1);
                        for (var j = 0; j < dmesgData.Length; ++j) {
                            if (dmesgData[j].ContainsIgnoreCase(busvalue)) {
                                var match = _ramPattern.Match(dmesgData[j]);
                                if (match.Success) {
                                    vram = GetRamValueMB(match.Value);
                                }
                            }
                        }
                    }
                }
                _videoCardInfo.Add(new VideoCardInfo() { VideoCardName = productName, VideoRAM = vram, VideoProcessor = vendorName });
            }
        }

        private static IEnumerable<IDictionary<string,string>> ParseDisplayData(IEnumerable<string> lines) {
            if (lines.Count() == 0) {
                yield break;
            }

            var seperatorMatch = _indentPattern.Match(lines.First());
            var separatorIndentation = 0;
            if (seperatorMatch.Success) {
                separatorIndentation = seperatorMatch.Groups["indent"].Length;
            } else {
                yield break;
            }

            Dictionary<string, string> data = null;
            foreach (var line in lines) {
                var match = _indentPattern.Match(line);
                if (match.Success && match.Groups["indent"].Length == separatorIndentation) {
                    if (data != null) {
                        yield return data;
                    }
                    data = new Dictionary<string, string>();
                    continue;
                }

                if (data == null) {
                    // input does not contain separators
                    yield break;
                }

                match = _keyValuePairsPattern.Match(line);
                if (match.Success) {
                    var key = match.Groups["key"].Value.Trim();
                    var value = match.Groups["value"].Value.Trim();
                    data[key] = value;
                }
            }

            if (data != null && data.Count() > 0) {
                yield return data;
            }
        }

        private static long GetRamValueMB(string vramStr) {
            long ram = 0;
            var match = _numPattern.Match(vramStr);
            
            if (match.Success) {
                long.TryParse(match.Value, out ram);
            }

            if (vramStr.ContainsIgnoreCase("G")) {
                ram *= 1024;
            }

            return ram;
        }

        private IEnumerable<string> ExecuteAndGetOutput(string command, string arguments) {
            IProcess proc = null;
            var standardOutData = new List<string>();
            try {
                var psi = new ProcessStartInfo() {
                    Arguments = arguments,
                    FileName = command,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                proc = _ps.Start(psi);

                while (!proc.StandardOutput.EndOfStream) {
                    standardOutData.Add(proc.StandardOutput.ReadLine());
                }
            } catch (Exception ex) {
                _logger.LogError(Resources.Error_FailedToRun.FormatInvariant($"{command} {arguments}", ex.Message));
            } finally {
                if (proc != null && !proc.HasExited) {
                    try {
                        proc?.Kill();
                    } catch (Exception ex) when (!ex.IsCriticalException()) {
                    }
                }
            }
            return standardOutData;
        }

        private static char[] _cpuInfoSplitter = new char[] { ' ', ':', '\t' };
        private (long user, long nice, long system, long idle, long iowait, long irq, long softirq, long steal) GetCurrentCpuUsage() {
            var cpuLoadInfo = "/proc/stat";
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
                var line = lines[index];
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
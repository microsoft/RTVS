// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;

namespace Microsoft.R.Platform.Host {
    public sealed class BrokerExecutableLocator {
        public const string WindowsBrokerName = "Microsoft.R.Host.Broker.Windows.exe";
        public const string UnixBrokerName = "Microsoft.R.Host.Broker.Unix.dll";
        public const string HostName = "Microsoft.R.Host";
        public const string WindowsExtension = ".exe";

        private readonly IFileSystem _fs;
        private readonly OSPlatform _platform;

        public static BrokerExecutableLocator Create(IFileSystem fs) {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                return new BrokerExecutableLocator(fs, OSPlatform.Windows);
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                return new BrokerExecutableLocator(fs, OSPlatform.OSX);
            }
            return new BrokerExecutableLocator(fs, OSPlatform.Linux);
        }

        public BrokerExecutableLocator(IFileSystem fs, OSPlatform platform) {
            _fs = fs;
            _platform = platform;
            BaseDirectory = Path.GetDirectoryName(typeof(BrokerExecutableLocator).GetTypeInfo().Assembly.GetAssemblyPath());
        }

        public string BaseDirectory { get; }

        public string GetBrokerExecutablePath() {
            // Broker can be Windows or .NET Core (Linux). NET Core broker 
            // does not have special folder, it sits where VS Code language 
            // server is since they share most of the .NET Core assemblies.
            if (_platform == OSPlatform.Windows) {
                var windowsVsBroker = Path.Combine(BaseDirectory, WindowsBrokerName);
                if (_fs.FileExists(windowsVsBroker)) {
                    return windowsVsBroker;
                }
            }

            var path = Path.Combine(BaseDirectory, GetBrokerMultiplatformSubpath());
            return _fs.FileExists(path) ? path : null;
        }

        public string GetHostExecutablePath() {
            // Host can be ..\..\Host\Windows (relative to the Windows broker)
            // or in Host/Platform (relative to the Unix broker)
            // or next to the broker (in remote services case)

            // Windows
            if (_platform == OSPlatform.Windows) {
                // Try next to the broker
                var hostName = HostName + WindowsExtension;
                var windowsHost = Path.Combine(BaseDirectory, hostName);
                if (_fs.FileExists(windowsHost)) {
                    return windowsHost;
                }
                // Try up above (VS IDE)
                windowsHost = Path.GetFullPath(Path.Combine(BaseDirectory, @"..\..\Host\Windows", hostName));
                if (_fs.FileExists(windowsHost)) {
                    return windowsHost;
                }
                // Try below broker (VS Code)
                windowsHost = Path.GetFullPath(Path.Combine(BaseDirectory, @"Host\Windows", hostName));
                if (_fs.FileExists(windowsHost)) {
                    return windowsHost;
                }
                return null;
            }

            // Unix
            // Try next to broker
            var unixHost = Path.Combine(BaseDirectory, HostName);
            if (_fs.FileExists(unixHost)) {
                return unixHost;
            }
            // VS Code on Unix (Host/Mac or Host/Linux)
            var path = Path.Combine(BaseDirectory, GetUnixMultiplatformSubPath());
            return _fs.FileExists(path) ? path : null;
        }

        private string GetBrokerMultiplatformSubpath() {
            if (_platform == OSPlatform.Windows) {
                return @"Broker\Windows\" + WindowsBrokerName;
            }
            return UnixBrokerName;
        }

        private string GetUnixMultiplatformSubPath() {
            string folder;
            if (_platform == OSPlatform.OSX) {
                folder = "/Mac/";
            } else {
                folder = "/Linux/";
            }
            return "Host" + folder + HostName;
        }
    }
}

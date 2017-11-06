// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;

namespace Microsoft.R.Platform.Host {
    public sealed class BrokerExecutableLocator {
        private const string WindowsBrokerName = "Microsoft.R.Host.Broker.Windows.exe";
        private const string UnixBrokerName = "Microsoft.R.Host.Broker.Unix.dll";
        private const string HostName = "Microsoft.R.Host";
        private const string WindowsExtension = ".exe";
        private readonly string _baseDirectory;
        private readonly IFileSystem _fs;

        public BrokerExecutableLocator(IFileSystem fs) {
            _fs = fs;
            _baseDirectory = Path.GetDirectoryName(typeof(BrokerExecutableLocator).GetTypeInfo().Assembly.GetAssemblyPath());
            var index = _baseDirectory.IndexOfIgnoreCase("server");
            if(index > 0) {
                // VS Code case
                _baseDirectory = _baseDirectory.Substring(0, index + 6);
            }
        }

        public string GetBrokerExecutablePath() {
            // Broker can be Windows or .NET Core (Linux). NET Core broker 
            // does not have special folder, it sits where VS Code language 
            // server is since they share most of the .NET Core assemblies.
            var windowsVsBroker = Path.Combine(_baseDirectory, WindowsBrokerName);
            return _fs.FileExists(windowsVsBroker)
                ? windowsVsBroker
                : Path.Combine(_baseDirectory, GetBrokerMultiplatformSubpath());
        }

        public string GetHostExecutablePath() {
            // Host can be ..\..\Host\Windows (relative to the Windows broker)
            // or in Host/Platform (relative to the Unix broker)
            // or next to the broker (in remote services case)

            // Windows
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                // Try next to the broker
                var hostName = HostName + WindowsExtension;
                var windowsHost = Path.Combine(_baseDirectory, hostName);
                if(_fs.FileExists(windowsHost)) {
                    return windowsHost;
                }
                // Try up above (VS IDE)
                windowsHost = Path.GetFullPath(Path.Combine(_baseDirectory, @"..\..\Host\Windows", hostName));
                if (_fs.FileExists(windowsHost)) {
                    return windowsHost;
                }
                // Try below broker (VS Code)
                windowsHost = Path.GetFullPath(Path.Combine(_baseDirectory, @"Host\Windows", hostName));
                if (_fs.FileExists(windowsHost)) {
                    return windowsHost;
                }
                return null;
            }

            // Unix
            // Try next to broker
            var unixHost = Path.Combine(_baseDirectory, HostName);
            if (_fs.FileExists(unixHost)) {
                return unixHost;
            }
            // VS Code on Unix (Host/Mac or Host/Linux)
            return Path.Combine(_baseDirectory, GetUnixMultiplatformSubPath());
        }

        private static string GetBrokerMultiplatformSubpath() {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                return @"Broker\Windows\" + WindowsBrokerName;
            }
            return UnixBrokerName;
        }

        private static string GetUnixMultiplatformSubPath() {
            string folder;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                folder = "/Mac/";
            } else {
                folder = "/Linux/";
            }
            return "Host" + folder + HostName;
        }
    }
}

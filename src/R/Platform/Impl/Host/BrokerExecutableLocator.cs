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
        private const string LinuxBrokerName = "Microsoft.R.Host.Broker.Linux.dll";
        private const string HostName = "Microsoft.R.Host";
        private const string WindowsExtension = ".exe";
        private readonly string _baseDirectory;
        private readonly IFileSystem _fs;

        public BrokerExecutableLocator(IFileSystem fs) {
            _fs = fs;
            _baseDirectory = Path.GetDirectoryName(typeof(BrokerExecutableLocator).GetTypeInfo().Assembly.GetAssemblyPath());
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
            var windowsVsHost = Path.Combine(_baseDirectory, HostName + WindowsExtension);
            return _fs.FileExists(windowsVsHost)
                ? windowsVsHost
                : Path.Combine(_baseDirectory, GetHostMultiplatformSubPath());
        }

        private static string GetBrokerMultiplatformSubpath() {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                return @"Broker\Windows\" + WindowsBrokerName;
            }
            return LinuxBrokerName;
        }

        private static string GetHostMultiplatformSubPath() {
            var extension = string.Empty;
            string folder;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                folder = @"\Windows\";
                extension = WindowsExtension;
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                folder = "/Mac/";
            } else {
                folder = "/Linux/";
            }
            return "Host" + folder + HostName + extension;
        }
    }
}

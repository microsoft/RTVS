// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;

namespace Microsoft.R.Platform.Host {
    public sealed class BrokerExecutableLocator {
        private const string RHostBrokerBaseName = "Microsoft.R.Host.Broker";
        private const string RHostExe = "Microsoft.R.Host.exe";
        private readonly string _baseDirectory;
        private readonly IFileSystem _fs;

        public BrokerExecutableLocator(IFileSystem fs) {
            _fs = fs;
            _baseDirectory = Path.GetDirectoryName(typeof(BrokerExecutableLocator).GetTypeInfo().Assembly.GetAssemblyPath());
        }

        public string GetBrokerExecutablePath() {
            var platformName = GetPlatformName();
            var brokerBinaryName = RHostBrokerBaseName + "." + platformName + GetExecutableExtension();
            return Path.Combine(_baseDirectory, @"Broker\", platformName, brokerBinaryName);
        }

        public string GetHostExecutablePath() {
            // Two cases: called from local client, paths are
            //      Host/Platform/*.exe
            //
            // When called from broker it is
            //      ../../Host/Platform/*.exe
            if (_fs.DirectoryExists("Host")) {
                return Path.Combine(_baseDirectory, "Host" + Path.DirectorySeparatorChar, GetPlatformName(), RHostExe);
            }

            var relativePath = $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Host{Path.DirectorySeparatorChar}";
            return Path.GetFullPath(Path.Combine(_baseDirectory, relativePath, GetPlatformName(), RHostExe));
        }

        private static string GetPlatformName() {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                return "Windows";
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                return "Linux";
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
                return "OSX";
            }
            Debug.Fail("GetPlatformName: Unknown OS type");
            return string.Empty;
        }

        private static string GetExecutableExtension() 
            => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : ".dll";
    }
}

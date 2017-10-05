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
        private readonly string _baseDirectory;
        private readonly IFileSystem _fs;

        public BrokerExecutableLocator(IFileSystem fs) {
            _fs = fs;
            _baseDirectory = Path.GetDirectoryName(typeof(BrokerExecutableLocator).GetTypeInfo().Assembly.GetAssemblyPath());
        }

        public string GetBrokerExecutablePath() {
            var platformName = GetPlatformName();
            var brokerBinaryName = GetBrokerExecutableName();
            if (_fs.DirectoryExists(Path.Combine(_baseDirectory, @"Broker\"))) {
                 return Path.Combine(_baseDirectory, @"Broker\", platformName, brokerBinaryName);
            }
            return Path.Combine(_baseDirectory, brokerBinaryName);
        }

        public string GetHostExecutablePath() {
            // Two cases: called from local client, paths are
            //      Host/Platform/*.exe
            //
            // When called from broker it is
            //      ../../Host/Platform/*.exe
            var rHostExeName = GetNativeExecutableName();
            var hostPath = Path.Combine(_baseDirectory, rHostExeName);
            if (_fs.FileExists(hostPath)) {
                return Path.Combine(_baseDirectory, rHostExeName);
            }

            var hostDirectory = Path.Combine(_baseDirectory, "Host" + Path.DirectorySeparatorChar);
            if (_fs.DirectoryExists(hostDirectory)) {
                return Path.Combine(_baseDirectory, hostDirectory, GetPlatformName(), rHostExeName);
            }

            var relativePath = $"..{Path.DirectorySeparatorChar}..{Path.DirectorySeparatorChar}Host{Path.DirectorySeparatorChar}";
            return Path.GetFullPath(Path.Combine(_baseDirectory, relativePath, GetPlatformName(), rHostExeName));
        }

        private static string GetPlatformName() 
            => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" : "Unix";

        private static string GetBrokerExecutableName() {
            var name = "Microsoft.R.Host.Broker." + GetPlatformName();
            return name + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : ".dll");
        }

        private static string GetNativeExecutableName() {
            const string name = "Microsoft.R.Host";
            return name + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : string.Empty);
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Common.Core;

namespace Microsoft.R.Host.Client.Host {
    internal sealed class BrokerExecutableLocator {
        private const string RHostBrokerBaseName = "Microsoft.R.Host.Broker";
        private const string RHostExe = "Microsoft.R.Host.exe";
        private readonly string _baseDirectory;

        public BrokerExecutableLocator() {
            _baseDirectory = Path.GetDirectoryName(typeof(RHost).GetTypeInfo().Assembly.GetAssemblyPath());
        }

        public string GetBrokerExecutablePath() {
            var platformName = GetPlatformName();
            var brokerBinaryName = RHostBrokerBaseName + platformName + GetExecutableExtension();
            return Path.Combine(_baseDirectory, @"Broker\", platformName, brokerBinaryName);
        }

        public string GetHostExecutablePath() 
            => Path.Combine(_baseDirectory, @"Host\", GetPlatformName(), RHostExe);

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

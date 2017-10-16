// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.IO;
using Microsoft.Common.Core.IO;
using Microsoft.R.Platform.IO;

namespace Microsoft.UnitTests.Core.Linux {
    public class TestLinuxFileSystem : FileSystem, IFileSystem {
        readonly UnixFileSystem _fs = new UnixFileSystem();
        private readonly bool _doPlatformDefault;

        public TestLinuxFileSystem(bool doPlatformDefault = true) {
            _doPlatformDefault = doPlatformDefault;
        }

        IEnumerable<string> IFileSystem.FileReadAllLines(string path) {
            switch (path) {
                case "/var/lib/dpkg/status":
                    return File.ReadAllLines("TestData/status"); // this is the test status file
                case "/var/lib/dpkg/info/microsoft-r-open-mro-3.3.list":
                    return File.ReadAllLines("TestData/microsoft-r-open-mro-3.3.list"); // this is the test status file
                case "/var/lib/dpkg/info/r-base-core.list":
                    return File.ReadAllLines("TestData/r-base-core.list"); // this is the test status file
                default:
                    return _fs.FileReadAllLines(path);
            }
        }

        bool IFileSystem.FileExists(string path) {
            switch (path) {
                case "/var/lib/dpkg/status":
                case "/var/lib/dpkg/info/microsoft-r-open-mro-3.3.list":
                case "/var/lib/dpkg/info/r-base-core.list":
                case "/usr/lib64/microsoft-r/3.3/lib64/R/lib/libR.so":
                case "/usr/lib/R/lib/libR.so":
                    return true;
                default:
                    return _fs.FileExists(path);
            }
        }

        bool IFileSystem.DirectoryExists(string fullPath) {
            switch (fullPath) {
                case "/usr/lib64/microsoft-r/3.3/lib64/R":
                case "/usr/lib64/microsoft-r/3.3/lib64/R/lib":
                case "/usr/lib/R":
                case "/usr/lib/R/lib":
                    return true;
                default:
                    return _fs.DirectoryExists(fullPath);
            }
        }

        string[] IFileSystem.GetFiles(string path, string pattern, SearchOption option) {
            switch (pattern) {
                case "microsoft-r-open-mro-3.3*.list":
                    return new string[] { "TestData/microsoft-r-open-mro-3.3.list" }; // this is the test status file
                case "/var/lib/dpkg/info/r-base-core.list":
                    return new string[] { "TestData/r-base-core.list" }; // this is the test status file
            }
            return _doPlatformDefault ? _fs.GetFiles(path, pattern, option) : new string[] { };
        }
    }
}

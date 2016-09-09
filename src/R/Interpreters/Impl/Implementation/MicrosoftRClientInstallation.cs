// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Linq;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.OS;
using Microsoft.Win32;
using static System.FormattableString;

namespace Microsoft.R.Interpreters {
    public static class MicrosoftRClientInstallation {
        private const string _rClientKey = @"SOFTWARE\Microsoft\R Client";
        private const string _rServer = "R_SERVER";

        public static string GetRClientPath(IRegistry registry = null) {
            registry = registry ?? new RegistryImpl();
            try {
                using (var hkcu = registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)) {
                    using (var key = hkcu.OpenSubKey(_rClientKey)) {
                        string path = (string)key.GetValue("Path");
                        if (!string.IsNullOrEmpty(path)) {
                            return Path.Combine(path, _rServer + "\\");
                        }
                    }
                }
            } catch (Exception) { }

            return string.Empty;
        }

        public static IRInterpreterInfo GetMicrosoftRClientInfo(IRegistry registry = null, IFileSystem fileSystem = null) {
            registry = registry ?? new RegistryImpl();
            fileSystem = fileSystem ?? new FileSystem();

            var info = GetMRCInfoFromSQL(registry, fileSystem);
            if (info == null) {
                info = GetMRCInfoFromRCore(registry, fileSystem);
            }
            return info;
        }

        private static IRInterpreterInfo GetMRCInfoFromRCore(IRegistry registry, IFileSystem fileSystem) {
            var engines = new RInstallation().GetCompatibleEngines();
            return engines.First();
        }

        private static IRInterpreterInfo GetMRCInfoFromSQL(IRegistry registry, IFileSystem fileSystem) {
            // First check that MRS is present on the machine.
            bool mrsInstalled = false;

            try {
                using (var hklm = registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)) {
                    using (var key = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\130\sql_shared_mr")) {
                        var path = (string)key?.GetValue("Path");
                        if (!string.IsNullOrEmpty(path) && path.Contains(_rServer)) {
                            mrsInstalled = true;
                        }
                    }
                }
            } catch (Exception) { }

            // If yes, check 32-bit registry for R engine installed by the R Server.
            // TODO: remove this when MRS starts writing 64-bit keys.
            if (mrsInstalled) {
                using (IRegistryKey hklm = registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)) {
                    try {
                        using (var key = hklm.OpenSubKey(@"SOFTWARE\R-core\R64")) {
                            foreach (var keyName in key.GetSubKeyNames()) {
                                using (var rsKey = key.OpenSubKey(keyName)) {
                                    try {
                                        var path = (string)rsKey?.GetValue("InstallPath");
                                        if (!string.IsNullOrEmpty(path) && path.Contains(_rServer)) {
                                            var info = new RInterpreterInfo(string.Empty, path);
                                            if (info.VerifyInstallation(new SupportedRVersionRange(), fileSystem)) {
                                                return new RInterpreterInfo(Invariant($"Microsoft R Client ({info.Version.Major}.{info.Version.Minor}.{info.Version.Build})"), info.InstallPath);
                                            }
                                        }
                                    } catch (Exception) { }
                                }
                            }
                        }
                    } catch (Exception) { }
                }
            }

            return null;
        }
    }
}

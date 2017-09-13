// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using Microsoft.Common.Core.OS;
using Microsoft.Win32;

namespace Microsoft.R.Interpreters {
    public static class SqlRClientInstallation {
        private const string _rClientKey = @"SOFTWARE\Microsoft\R Client";
        private const string _rServer = "R_SERVER";

        public static string GetRClientPath(IRegistry registry = null) {
            registry = registry ?? new RegistryImpl();
            try {
                using (var hkcu = registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)) {
                    using (var key = hkcu.OpenSubKey(_rClientKey)) {
                        string path = (string)key?.GetValue("Path");
                        if (!string.IsNullOrEmpty(path)) {
                            return Path.Combine(path, _rServer + "\\");
                        }
                    }
                }
            } catch (Exception) { }

            return string.Empty;
        }
    }
}

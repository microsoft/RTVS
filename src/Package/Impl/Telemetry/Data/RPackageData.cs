// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.R.Host.Client.Install;
using Microsoft.R.Support.Settings;

namespace Microsoft.VisualStudio.R.Package.Telemetry.Data {
    /// <summary>
    /// Represents R package data as reported in telemetry
    /// </summary>
    internal static class RPackageData {
        /// <summary>
        /// Retrieves hashes for all R package names in a given folder
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetInstalledPackageHashes(RPackageType packageType) {

            string rInstallPath = RInstallation.GetRInstallPath(RToolsSettings.Current.RBasePath, null);
            if (!string.IsNullOrEmpty(rInstallPath)) {
                IEnumerable<string> packageNames = Enumerable.Empty<string>();
                if (packageType == RPackageType.Base) {
                    packageNames = FolderUtility.GetSubfolderRelativePaths(Path.Combine(rInstallPath, "library"));
                } else {
                    Version v = RInstallation.GetRVersionFromFolderName(rInstallPath.Substring(rInstallPath.LastIndexOf('\\') + 1));
                    if (v.Major > 0) {
                        string userLibraryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                                              @"R\win-library\", v.Major.ToString() + "." + v.Minor.ToString());

                        packageNames = FolderUtility.GetSubfolderRelativePaths(userLibraryPath);
                    }
                }

                foreach (string p in packageNames) {
                    string hash = CalculateMD5Hash(p);
                    yield return hash;
                }
            }
        }

        private static string CalculateMD5Hash(string input) {
            SHA512 sha = SHA512.Create();
            byte[] inputBytes = Encoding.Unicode.GetBytes(input);
            byte[] hash = sha.ComputeHash(inputBytes);

            return BitConverter.ToString(hash);
        }
    }
}

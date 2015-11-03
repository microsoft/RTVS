using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Support.Settings;
using Microsoft.Win32;

namespace Microsoft.R.Support.Utility {
    /// <summary>
    /// Verifies that R is installed in the folder
    /// specified in settings. If nothing is specified
    /// settings try and find highest version.
    /// </summary>
    public static class RInstallation {
        public static bool VerifyRIsInstalled() {
            string rPath = RInstallation.GetRInstallPath();
            if (!string.IsNullOrEmpty(rPath)) {
                bool rExeExists = File.Exists(Path.Combine(rPath, @"bin\R.exe"));
                bool rTermExists = File.Exists(Path.Combine(rPath, @"bin\x64\RTerm.exe"));

                if (rExeExists && rTermExists) {
                    return true;
                }
            }

            string message = string.Format(CultureInfo.InvariantCulture, Resources.Error_RNotInstalled);
            EditorShell.Current.ShowErrorMessage(message);

            Process.Start("https://cran.r-project.org");

            return false;
        }

        public static void GoToRInstallPage() {
            Process.Start("https://cran.r-project.org/");
        }

        /// <summary>
        /// Retrieves path to the installed R engine root folder.
        /// First tries user settings, then 64-bit registry.
        /// </summary>
        public static string GetRInstallPath() {
            string rBasePath = RToolsSettings.Current != null ? RToolsSettings.Current.RBasePath : null;

            if (string.IsNullOrEmpty(rBasePath) || !Directory.Exists(rBasePath)) {
                rBasePath = RInstallation.GetLatestEnginePathFromRegistry();
            }

            return rBasePath;
        }

        /// <summary>
        /// Retrieves path to the installed R engine binaries folder.
        /// R version is retrieved from settings or, af none is set,
        /// highest version is retrieved from registry.
        /// </summary>
        public static string GetBinariesFolder() {
            string binFolder = null;
            string installPath = RInstallation.GetRInstallPath();

            if (!string.IsNullOrEmpty(installPath)) {
                    binFolder = Path.Combine(installPath, @"bin\x64");
                }

            return binFolder;
        }

        /// <summary>
        /// Retrieves path to the latest (highest version) R installation
        /// from registry. Typically in the form 'Program Files\R\R-3.2.1'
        /// </summary>
        public static string GetLatestEnginePathFromRegistry() {
            string[] installedEngines = GetInstalledEngineVersionsFromRegistry();
            string highestVersionName = string.Empty;
            Version highest = null;

            foreach (string name in installedEngines) {
                Version v = new Version(name);
                if (highest != null) {
                    if (v > highest) {
                        highest = v;
                        highestVersionName = name;
                    }
                } else {
                    highest = v;
                    highestVersionName = name;
                }
            }

            return GetRVersionInstallPathFromRegistry(highestVersionName);
        }

        /// <summary>
        /// Retrieves installed R versions. Returns array of strings
        /// that typically look like 'R-3.2.1' and typically are
        /// subfolders of 'Program Files\R'
        /// </summary>
        public static string[] GetInstalledEngineVersionsFromRegistry() {
            List<string> enginePaths = new List<string>();

            // HKEY_LOCAL_MACHINE\SOFTWARE\R-core
            // HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\R-core
            using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)) {
                RegistryKey rKey = null;

                try {
                    rKey = hklm.OpenSubKey(@"SOFTWARE\R-core\R");
                    if (rKey == null) {
                        // Possibly 64-bit machine with only 32-bit R installed
                        // This is not supported as we require 64-bit R.
                        // rKey = hklm.OpenSubKey(@"SOFTWARE\Wow6432Node\R-core\R");
                    }

                    if (rKey != null) {
                        return rKey.GetSubKeyNames();
                    }
                } catch (Exception) { } finally {
                    if (rKey != null) {
                        rKey.Dispose();
                    }
                }
            }

            return new string[0];
        }

        private static string GetRVersionInstallPathFromRegistry(string version) {
            // HKEY_LOCAL_MACHINE\SOFTWARE\R-core
            using (RegistryKey hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)) {
                using (var rKey = hklm.OpenSubKey(@"SOFTWARE\R-core\R\" + version)) {
                    if (rKey != null) {
                        return rKey.GetValue("InstallPath") as string;
                    }
                }
            }

            return string.Empty;
        }
    }
}

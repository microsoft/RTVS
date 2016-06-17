// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Telemetry;
using Microsoft.R.Host.Client.Telemetry;
using Microsoft.Win32;

namespace Microsoft.R.Host.Client.Install {
    /// <summary>
    /// Verifies that R is installed in the folder
    /// specified in settings. If nothing is specified
    /// settings try and find highest version.
    /// </summary>
    public static class RInstallation {
        private const string rServer = "R_SERVER";
        private static string[] rFolders = new string[] { "MRO", "RRO", "R" };
        private static IRegistry _registry;
        private static IFileSystem _fileSystem;
        private static IProcessServices _processServices;

        internal static IRegistry Registry {
            get {
                if (_registry == null) {
                    _registry = new RegistryImpl();
                }
                return _registry;
            }
            set { _registry = value; }
        }

        internal static IFileSystem FileSystem {
            get {
                if (_fileSystem == null) {
                    _fileSystem = new FileSystem();
                }
                return _fileSystem;
            }
            set { _fileSystem = value; }
        }

        internal static IProcessServices ProcessServices {
            get {
                if (_processServices == null) {
                    _processServices = new ProcessServices();
                }
                return _processServices;
            }
            set { _processServices = value; }
        }


        /// <summary>
        /// Tries to determine R installation information. If user-specified path
        /// is supplied, it is used. If not, registry is used. If nothing is found
        /// in the registry, makes attempt to find compatible 64-bit installation 
        /// of MRO, RRO or R (in this order) in Program Files folder
        /// </summary>
        /// <param name="basePath">Path as specified by the user settings</param>
        /// <returns></returns>
        public static RInstallData GetInstallationData(
            string basePath,
            ISupportedRVersionRange svl) {

            string path = GetRInstallPath(basePath, svl);

            // If nothing is found, look into the file system
            if (string.IsNullOrEmpty(path)) {
                foreach (var f in rFolders) {
                    path = TryFindRInProgramFiles(f, svl);
                    if (!string.IsNullOrEmpty(path)) {
                        break;
                    }
                }
            }

            // Still nothing? Fail, caller will typically display an error message.
            if (string.IsNullOrEmpty(path)) {
                return new RInstallData() { Status = RInstallStatus.PathNotSpecified };
            }

            // Now verify if files do exist and are of the correct version.
            // There may be cases when R was not fully uninstalled or when
            // version claimed in the registry is not what is really in files.
            RInstallData data = new RInstallData() { Status = RInstallStatus.OK, Path = path };

            // Normalize path so it points to R root and not to bin or bin\x64
            path = NormalizeRPath(path);
            try {
                string rDirectory = Path.Combine(path, @"bin\x64");
                data.BinPath = rDirectory;

                string rDllPath = Path.Combine(rDirectory, "R.dll");
                string rGraphAppPath = Path.Combine(rDirectory, "Rgraphapp.dll");
                string rTermPath = Path.Combine(rDirectory, "RTerm.exe");
                string rScriptPath = Path.Combine(rDirectory, "RScript.exe");
                string rGuiPath = Path.Combine(rDirectory, "RGui.exe");

                if (FileSystem.FileExists(rDllPath) && FileSystem.FileExists(rTermPath) &&
                    FileSystem.FileExists(rScriptPath) && FileSystem.FileExists(rGraphAppPath) &&
                    FileSystem.FileExists(rGuiPath)) {

                    data.Version = GetRVersion(path);
                    if (!svl.IsCompatibleVersion(data.Version)) {
                        data.Status = RInstallStatus.UnsupportedVersion;
                    }
                } else {
                    data.Status = RInstallStatus.NoRBinaries;
                }
            } catch (ArgumentException aex) {
                data.Status = RInstallStatus.ExceptionAccessingPath;
                data.Exception = aex;
            } catch (IOException ioex) {
                data.Status = RInstallStatus.ExceptionAccessingPath;
                data.Exception = ioex;
            }

            return data;
        }

        public static Version GetRVersion(string basePath) {
            string rDllPath = Path.Combine(basePath, @"bin\x64\R.dll");
            IFileVersionInfo fvi = FileSystem.GetVersionInfo(rDllPath);
            int minor, revision;

            GetRVersionPartsFromFileMinorVersion(fvi.FileMinorPart, out minor, out revision);
            return new Version(fvi.FileMajorPart, minor, revision);
        }

        public static string NormalizeRPath(string path) {
            string[] suffixes = { @"\bin", @"\bin\x64" };
            foreach (var s in suffixes) {
                if (path.EndsWith(s, StringComparison.OrdinalIgnoreCase)) {
                    path = path.Substring(0, path.Length - s.Length);
                    break;
                }
            }
            return path;
        }

        /// <summary>
        /// Retrieves path to the installed R engine root folder.
        /// First tries user settings, then 64-bit registry.
        /// </summary>
        public static string GetRInstallPath(string basePath, ISupportedRVersionRange svl = null) {
            svl = svl ?? new SupportedRVersionRange();
            if (string.IsNullOrEmpty(basePath) || !FileSystem.DirectoryExists(basePath)) {
                basePath = GetRPathFromMRS();
                if (string.IsNullOrEmpty(basePath)) {
                    basePath = GetCompatibleEnginePathFromRegistry(svl);
                }
            }
            return basePath;
        }

        private static string GetRPathFromMRS() {
            // First check that MRS is present on the machine.
            bool mrsInstalled = false;
            try {
                using (var hklm = Registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)) {
                    using (var key = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\130\sql_shared_mr")) {
                        var path = (string)key.GetValue("Path");
                        if (!string.IsNullOrEmpty(path) && path.Contains(rServer)) {
                            mrsInstalled = true;
                        }
                    }
                }
            } catch (Exception) { }

            // If yes, check 32-bit registry for R engine installed by the R Server.
            // TODO: remove this when MRS starts writing 64-bit keys.
            if (mrsInstalled) {
                using (IRegistryKey hklm = Registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)) {
                    try {
                        using (var key = hklm.OpenSubKey(@"SOFTWARE\R-core\R64")) {
                            foreach (var keyName in key.GetSubKeyNames()) {
                                using (var rsKey = key.OpenSubKey(keyName)) {
                                    try {
                                        var path = (string)rsKey.GetValue("InstallPath");
                                        if (!string.IsNullOrEmpty(path) && path.Contains(rServer)) {
                                            return path;
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

        /// <summary>
        /// Retrieves path to the installed R engine binaries folder.
        /// R version is retrieved from settings or, af none is set,
        /// highest version is retrieved from registry.
        /// </summary>
        public static string GetBinariesFolder(string basePath, ISupportedRVersionRange svl) {
            string binFolder = null;
            string installPath = RInstallation.GetRInstallPath(basePath, svl);

            if (!String.IsNullOrEmpty(installPath)) {
                binFolder = Path.Combine(installPath, @"bin\x64");
            }

            return binFolder;
        }

        /// <summary>
        /// Given R minor file version like 10 converts it to R engine minor version.
        /// For example, file may have version 3.10 which means R 3.1.0. In turn,
        /// file version 2.125 means R engine version is 2.12.5.
        /// </summary>
        /// <param name="minorVersion"></param>
        /// <param name="minor"></param>
        /// <param name="revision"></param>
        private static void GetRVersionPartsFromFileMinorVersion(int minorVersion, out int minor, out int revision) {
            revision = minorVersion % 10;
            if (minorVersion < 100) {
                minor = minorVersion / 10;
            } else {
                minor = minorVersion / 100;
            }
        }

        /// <summary>
        /// Retrieves path to the latest (highest version) R installation
        /// from registry. Typically in the form 'Program Files\R\R-3.2.1'
        /// Selects highest from compatible versions, not just the highest.
        /// </summary>
        public static string GetCompatibleEnginePathFromRegistry(ISupportedRVersionRange svl = null) {
            svl = svl ?? new SupportedRVersionRange();
            string[] installedEngines = GetInstalledEngineVersionsFromRegistry();
            string highestVersionName = String.Empty;
            Version highest = null;

            foreach (string name in installedEngines) {
                // Protect from random key name format changes
                if (!String.IsNullOrEmpty(name)) {
                    string versionString = ExtractVersionString(name);
                    Version v;
                    if (Version.TryParse(versionString, out v) && svl.IsCompatibleVersion(v)) {
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
                }
            }

            return GetRVersionInstallPathFromRegistry(highestVersionName);
        }

        private static string ExtractVersionString(string original) {
            int start = 0;
            int end = original.Length;

            for (; start < original.Length; start++) {
                if (Char.IsDigit(original[start])) {
                    break;
                }
            }

            for (; end > 0; end--) {
                if (Char.IsDigit(original[end - 1])) {
                    break;
                }
            }

            return end > start ? original.Substring(start, end - start) : String.Empty;
        }

        /// <summary>
        /// Retrieves installed R versions. Returns array of strings
        /// that typically look like 'R-3.2.1' (but development versions 
        /// may also look like '3.3.0 Pre-release' and typically are
        /// subfolders of 'Program Files\R'
        /// </summary>
        public static string[] GetInstalledEngineVersionsFromRegistry() {
            List<string> enginePaths = new List<string>();

            // HKEY_LOCAL_MACHINE\SOFTWARE\R-core
            // HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\R-core
            // HKEY_LOCAL_MACHINE\SOFTWARE\R-core\R64\3.3.0 Pre-release
            using (IRegistryKey hklm = Registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)) {
                try {
                    using (var rKey = hklm.OpenSubKey(@"SOFTWARE\R-core\R")) {
                        return rKey.GetSubKeyNames();
                    }
                } catch (Exception) { }
            }

            return new string[0];
        }

        private static string GetRVersionInstallPathFromRegistry(string version) {
            // HKEY_LOCAL_MACHINE\SOFTWARE\R-core
            using (IRegistryKey hklm = Registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)) {
                try {
                    using (var rKey = hklm.OpenSubKey(@"SOFTWARE\R-core\R\" + version)) {
                        if (rKey != null) {
                            return rKey.GetValue("InstallPath") as string;
                        }
                    }
                } catch (Exception) { }
            }
            return String.Empty;
        }

        public static Version GetRVersionFromFolderName(string folderName) {
            if (folderName.StartsWith("R-", StringComparison.OrdinalIgnoreCase)) {
                try {
                    Version v;
                    if (Version.TryParse(folderName.Substring(2), out v)) {
                        return v;
                    }
                } catch (Exception) { }
            }
            return new Version(0, 0);
        }

        private static string TryFindRInProgramFiles(string folder, ISupportedRVersionRange supportedVersions) {
            string root = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            string baseRFolder = Path.Combine(root + @"Program Files\", folder);
            List<Version> versions = new List<Version>();
            try {
                IEnumerable<IFileSystemInfo> directories = FileSystem.GetDirectoryInfo(baseRFolder)
                                                                .EnumerateFileSystemInfos()
                                                                .Where(x => (x.Attributes & FileAttributes.Directory) != 0);
                foreach (IFileSystemInfo fsi in directories) {
                    string subFolderName = fsi.FullName.Substring(baseRFolder.Length + 1);
                    Version v = GetRVersionFromFolderName(subFolderName);
                    if (supportedVersions.IsCompatibleVersion(v)) {
                        versions.Add(v);
                    }
                }
            } catch (IOException) {
                // Don't do anything if there is no RRO installed
            }

            if (versions.Count > 0) {
                versions.Sort();
                Version highest = versions[versions.Count - 1];
                return Path.Combine(baseRFolder, String.Format(CultureInfo.InvariantCulture, "R-{0}.{1}.{2}", highest.Major, highest.Minor, highest.Build));
            }

            return string.Empty;
        }

        public static bool VerifyRIsInstalled(ICoreShell coreShell, ISupportedRVersionRange svl, string path, bool showErrors = true) {
            svl = svl ?? new SupportedRVersionRange();
            var data = RInstallation.GetInstallationData(path, svl);
            if (data.Status == RInstallStatus.OK) {
                return true;
            }

            if (showErrors) {
                if (ShowMessage(coreShell, data, svl) == MessageButtons.Yes) {
                    coreShell.TelemetryService.ReportEvent(TelemetryArea.Configuration, MrcTelemetryEvents.RClientInstallPrompt);
                    var installer = coreShell.ExportProvider.GetExportedValue<IMicrosoftRClientInstaller>();
                    installer.LaunchRClientSetup(coreShell);
                }
            }

            return false;
        }

        private static MessageButtons ShowMessage(ICoreShell coreShell, RInstallData data, ISupportedRVersionRange svl) {
            Debug.Assert(data.Status != RInstallStatus.OK);

            switch (data.Status) {
                case RInstallStatus.UnsupportedVersion:
                    return coreShell.ShowMessage(
                        string.Format(CultureInfo.InvariantCulture, Resources.Error_UnsupportedRVersion,
                        data.Version.Major, data.Version.Minor, data.Version.Build,
                        svl.MinMajorVersion, svl.MinMinorVersion, "*",
                        svl.MaxMajorVersion, svl.MaxMinorVersion, "*",
                        Environment.NewLine + Environment.NewLine),
                        MessageButtons.YesNo);

                case RInstallStatus.ExceptionAccessingPath:
                    coreShell.ShowErrorMessage(
                        string.Format(CultureInfo.InvariantCulture, Resources.Error_ExceptionAccessingPath,
                        data.Path, data.Exception.Message));
                    return MessageButtons.OK;

                case RInstallStatus.NoRBinaries:
                    return coreShell.ShowMessage(
                        string.Format(CultureInfo.InvariantCulture, Resources.Error_CannotFindRBinariesFormat,
                            data.Path, Environment.NewLine + Environment.NewLine, Environment.NewLine),
                        MessageButtons.YesNo);

                case RInstallStatus.PathNotSpecified:
                    return coreShell.ShowMessage(string.Format(CultureInfo.InvariantCulture,
                        Resources.Error_UnableToFindR, Environment.NewLine + Environment.NewLine), MessageButtons.YesNo);
            }
            return MessageButtons.OK;
        }
    }
}

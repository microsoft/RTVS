using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.OS;

namespace Microsoft.R.Actions.Utility {
    /// <summary>
    /// Verifies that R is installed in the folder
    /// specified in settings. If nothing is specified
    /// settings try and find highest version.
    /// </summary>
    public static class RInstallation {
        private static string[] rFolders = new string[] { "MRO", "RRO", "R" };
        private static IRegistry _registry;
        private static IFileSystem _fileSystem;

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

        /// <summary>
        /// Tries to determine R installation information. If user-specified path
        /// is supplied, it is used. If not, registry is used. If nothing is found
        /// in the registry, makes attempt to find compatible 64-bit installation 
        /// of MRO, RRO or R (in this order) in Program Files folder
        /// </summary>
        /// <param name="basePath">Path as specified by the user settings</param>
        /// <returns></returns>
        public static RInstallData GetInstallationData(string basePath, int minMajorVersion, int minMinorVersion, int maxMajorVersion, int maxMinorVersion) {
            string path = RInstallation.GetRInstallPath(basePath);

            // If nothing is found, look into the file system
            if (string.IsNullOrEmpty(path)) {
                foreach (var f in rFolders) {
                    path = TryFindRInProgramFiles(f, minMajorVersion, minMinorVersion, maxMajorVersion, maxMinorVersion);
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

            try {
                string rDirectory = Path.Combine(path, @"bin\x64");
                string rDllPath = Path.Combine(rDirectory, "R.dll");
                string rGraphAppPath = Path.Combine(rDirectory, "Rgraphapp.dll");
                string rTermPath = Path.Combine(rDirectory, "RTerm.exe");
                string rScriptPath = Path.Combine(rDirectory, "RScript.exe");
                string rGuiPath = Path.Combine(rDirectory, "RGui.exe");

                if (FileSystem.FileExists(rDllPath) && FileSystem.FileExists(rTermPath) &&
                    FileSystem.FileExists(rScriptPath) && FileSystem.FileExists(rGraphAppPath) &&
                    FileSystem.FileExists(rGuiPath)) {
                    IFileVersionInfo fvi = FileSystem.GetVersionInfo(rDllPath);
                    int minor, revision;

                    GetRVersionPartsFromFileMinorVersion(fvi.FileMinorPart, out minor, out revision);
                    data.Version = new Version(fvi.FileMajorPart, minor, revision);

                    if (!SupportedRVersionList.IsCompatibleVersion(data.Version)) {
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

        public static string NormalizeRPath(string path) {
            string bin64 = @"bin\x64";
            if (path.EndsWith(bin64, StringComparison.OrdinalIgnoreCase)) {
                path = path.Substring(0, path.Length - bin64.Length - 1);
            } else if (path.EndsWith("bin", StringComparison.OrdinalIgnoreCase)) {
                path = path.Substring(0, path.Length - 4);
            }
            return path;
        }

        public static void GoToRInstallPage() {
            Process.Start("https://mran.revolutionanalytics.com/download");
        }

        /// <summary>
        /// Retrieves path to the installed R engine root folder.
        /// First tries user settings, then 64-bit registry.
        /// </summary>
        public static string GetRInstallPath(string basePath) {
            if (string.IsNullOrEmpty(basePath) || !FileSystem.DirectoryExists(basePath)) {
                basePath = RInstallation.GetCompatibleEnginePathFromRegistry();
            }
            return basePath;
        }

        /// <summary>
        /// Retrieves path to the installed R engine binaries folder.
        /// R version is retrieved from settings or, af none is set,
        /// highest version is retrieved from registry.
        /// </summary>
        public static string GetBinariesFolder(string basePath) {
            string binFolder = null;
            string installPath = RInstallation.GetRInstallPath(basePath);

            if (!string.IsNullOrEmpty(installPath)) {
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
        public static string GetCompatibleEnginePathFromRegistry() {
            string[] installedEngines = GetInstalledEngineVersionsFromRegistry();
            string highestVersionName = string.Empty;
            Version highest = null;

            foreach (string name in installedEngines) {
                // Protect from random key name format changes
                if (!string.IsNullOrEmpty(name)) {
                    string versionString = ExtractVersionString(name);
                    Version v;
                    if (Version.TryParse(versionString, out v) && SupportedRVersionList.IsCompatibleVersion(v)) {
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

            return end > start ? original.Substring(start, end - start) : string.Empty;
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
            using (IRegistryKey hklm = Registry.OpenBaseKey(Win32.RegistryHive.LocalMachine, Win32.RegistryView.Registry64)) {
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
            using (IRegistryKey hklm = Registry.OpenBaseKey(Win32.RegistryHive.LocalMachine, Win32.RegistryView.Registry64)) {
                try {
                    using (var rKey = hklm.OpenSubKey(@"SOFTWARE\R-core\R\" + version)) {
                        if (rKey != null) {
                            return rKey.GetValue("InstallPath") as string;
                        }
                    }
                } catch (Exception) { }
            }
            return string.Empty;
        }

        public static Version GetRVersionFromFolderName(string folderName) {
            if (folderName.StartsWith("R-")) {
                try {
                    Version v;
                    if (Version.TryParse(folderName.Substring(2), out v)) {
                        return v;
                    }
                } catch (Exception) { }
            }
            return new Version(0, 0);
        }

        private static string TryFindRInProgramFiles(string folder, int minMajorVersion, int minMinorVersion, int maxMajorVersion, int maxMinorVersion) {
            string root = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            string baseRFolder = Path.Combine(root + @"Program Files\", folder);
            List<Version> versions = new List<Version>();
            try {
                IEnumerable<IFileSystemInfo> directories = FileSystem.GetDirectoryInfo(baseRFolder)
                                                                .EnumerateFileSystemInfos()
                                                                .Where(x => (x.Attributes & System.IO.FileAttributes.Directory) != 0);
                foreach (IFileSystemInfo fsi in directories) {
                    string subFolderName = fsi.FullName.Substring(baseRFolder.Length + 1);
                    Version v = GetRVersionFromFolderName(subFolderName);
                    if (v.Major >= minMajorVersion &&
                        v.Minor >= minMinorVersion &&
                        v.Major <= maxMajorVersion &&
                        v.Minor <= maxMinorVersion) {
                        versions.Add(v);
                    }
                }
            } catch (IOException) {
                // Don't do anything if there is no RRO installed
            }

            if (versions.Count > 0) {
                versions.Sort();
                Version highest = versions[versions.Count - 1];
                return Path.Combine(baseRFolder, string.Format(CultureInfo.InvariantCulture, "R-{0}.{1}.{2}", highest.Major, highest.Minor, highest.Build));
            }

            return string.Empty;
        }
    }
}

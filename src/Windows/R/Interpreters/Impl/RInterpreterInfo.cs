// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI;

namespace Microsoft.R.Interpreters {
    public sealed class RInterpreterInfo : IRInterpreterInfo {
        private readonly IFileSystem _fileSystem;
        private bool? _isValid;

        /// <summary>
        /// User-friendly name of the interpreter. Determined from the registry
        /// key for CRAN interpreters or via other means for MRO/MRC.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Found R version
        /// </summary>
        public Version Version { get; }

        /// <summary>
        /// Path to the R installation folder (without bin\x64)
        /// </summary>
        public string InstallPath { get; }

        /// <summary>
        /// Path to the R binaries (with bin\x64)
        /// </summary>
        public string BinPath { get; }

        public string DocPath { get; }

        public string IncludePath { get; }

        public string RShareDir { get; }

        public string[] SiteLibraryDirs { get; }

        public RInterpreterInfo(string name, string path, IFileSystem fileSystem) {
            _fileSystem = fileSystem;
            Name = name;
            InstallPath = NormalizeRPath(path);
            BinPath = Path.Combine(path, @"bin\x64");
            Version = DetermineVersion();
            DocPath = Path.Combine(path, "doc");
            IncludePath = Path.Combine(path, "include");
            RShareDir = Path.Combine(path, "share");
            SiteLibraryDirs = new string[0];
        }

        public bool VerifyInstallation(ISupportedRVersionRange svr = null, IServiceContainer services = null) {
            var ui = services?.GetService<IUIService>();
            if (_isValid.HasValue) {
                return _isValid.Value;
            }

            _isValid = false;

            svr = svr ?? new SupportedRVersionRange();

            // Normalize path so it points to R root and not to bin or bin\x64
            string rDllPath = Path.Combine(BinPath, "R.dll");
            string rGraphAppPath = Path.Combine(BinPath, "Rgraphapp.dll");
            string rTermPath = Path.Combine(BinPath, "RTerm.exe");
            string rScriptPath = Path.Combine(BinPath, "RScript.exe");
            string rGuiPath = Path.Combine(BinPath, "RGui.exe");

            try {
                if (_fileSystem.FileExists(rDllPath) && _fileSystem.FileExists(rTermPath) &&
                    _fileSystem.FileExists(rScriptPath) && _fileSystem.FileExists(rGraphAppPath) &&
                    _fileSystem.FileExists(rGuiPath)) {

                    var fileVersion = GetRVersionFromBinary(_fileSystem, rDllPath);
                    _isValid = IsSameVersion(fileVersion, Version) && svr.IsCompatibleVersion(Version);
                    if (!_isValid.Value) {
                        ui?.ShowMessage(
                            string.Format(CultureInfo.InvariantCulture, Resources.Error_UnsupportedRVersion,
                            Version.Major, Version.Minor, Version.Build, svr.MinMajorVersion, svr.MinMinorVersion, "*",
                            svr.MaxMajorVersion, svr.MaxMinorVersion, "*"), MessageButtons.OK);
                    }
                } else {
                    ui?.ShowMessage(string.Format(CultureInfo.InvariantCulture, Resources.Error_CannotFindRBinariesFormat, InstallPath), MessageButtons.OK);
                }
            } catch (Exception ex) when (ex is IOException || ex is ArgumentException || ex is UnauthorizedAccessException) {
                ui?.ShowErrorMessage(
                    string.Format(CultureInfo.InvariantCulture, Resources.Error_ExceptionAccessingPath, InstallPath, ex.Message));
            }

            return _isValid.Value;
        }

        private static bool IsSameVersion(Version v1, Version v2) {
            // Ignore build and revision
            return v1.Major == v2.Major && v1.Minor == v2.Minor;
        }

        private Version GetRVersionFromBinary(IFileSystem fs, string basePath) {
            string rDllPath = Path.Combine(BinPath, "R.dll");
            var fvi = fs.GetFileVersion(rDllPath);
            int minor, revision;

            GetRVersionPartsFromFileMinorVersion(fvi.Minor, out minor, out revision);
            return new Version(fvi.Major, minor, revision);
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

        private Version DetermineVersion() {
            Version v = null;

            string versionString = ExtractVersionString(Name);
            if (string.IsNullOrEmpty(versionString)) {
                // Try from file
                try {
                    string rDllPath = Path.Combine(BinPath, @"R.dll");
                    v = GetRVersionFromBinary(_fileSystem, rDllPath);
                } catch (IOException) { } catch (UnauthorizedAccessException) { }
            } else {
                Version.TryParse(versionString, out v);
            }

            return v ?? new Version();
        }

        private string ExtractVersionString(string original) {
            int start = 0;
            int end = original.Length;

            for (; start < original.Length; start++) {
                if (char.IsDigit(original[start])) {
                    break;
                }
            }

            for (; end > 0; end--) {
                if (char.IsDigit(original[end - 1])) {
                    break;
                }
            }

            return end > start ? original.Substring(start, end - start) : String.Empty;
        }
    }
}

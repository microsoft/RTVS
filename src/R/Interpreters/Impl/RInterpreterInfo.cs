// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;

namespace Microsoft.R.Interpreters {
    internal sealed class RInterpreterInfo : IRInterpreterInfo {
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

        public RInstallStatus CheckInstallation(IFileSystem fs, bool showErrors) {
            // Normalize path so it points to R root and not to bin or bin\x64
            string rDllPath = Path.Combine(BinPath, "R.dll");
            string rGraphAppPath = Path.Combine(BinPath, "Rgraphapp.dll");
            string rTermPath = Path.Combine(BinPath, "RTerm.exe");
            string rScriptPath = Path.Combine(BinPath, "RScript.exe");
            string rGuiPath = Path.Combine(BinPath, "RGui.exe");

            try {
                if (fs.FileExists(rDllPath) && fs.FileExists(rTermPath) &&
                    fs.FileExists(rScriptPath) && fs.FileExists(rGraphAppPath) &&
                    fs.FileExists(rGuiPath)) {

                    var fileVersion = GetRVersionFromBinary(rDllPath, fs);
                    if (fileVersion != Version) {
                        return RInstallStatus.UnsupportedVersion;
                    }
                } else {
                    return RInstallStatus.NoRBinaries;
                }
            } catch(IOException ioex) {

            } catch (ArgumentException aex) {

            } catch(UnauthorizedAccessException uaex) {

            }

            return RInstallStatus.OK;
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

        private Version GetRVersionFromBinary(string basePath, IFileSystem fs) {
            string rDllPath = Path.Combine(BinPath, @"R.dll");
            IFileVersionInfo fvi = fs.GetVersionInfo(rDllPath);
            int minor, revision;

            GetRVersionPartsFromFileMinorVersion(fvi.FileMinorPart, out minor, out revision);
            return new Version(fvi.FileMajorPart, minor, revision);
        }

        private static string NormalizeRPath(string path) {
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

        public RInterpreterInfo(string name, string path) {
            InstallPath = NormalizeRPath(path);
            BinPath = System.IO.Path.Combine(path, @"bin\x64");
            Version = DetermineVersion();
        }

        private Version DetermineVersion() {
            string versionString = ExtractVersionString(Name);
            Version v;
            if (string.IsNullOrEmpty(versionString) || !Version.TryParse(versionString, out v)) {
                v = new Version();
            }
            return v;
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

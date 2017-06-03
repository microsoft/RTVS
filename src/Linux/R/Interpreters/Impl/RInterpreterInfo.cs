// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI;
using Microsoft.Common.Core.OS;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.R.Interpreters {
    public sealed class RInterpreterInfo : IRInterpreterInfo {
        private readonly IFileSystem _fileSystem;
        private bool? _isValid;
        private readonly string _packageFullVersion;

        public string Name { get; }

        public Version Version { get; }

        /// <summary>
        /// Path to the /R directory that contains libs, doc, etc
        /// </summary>
        public string InstallPath { get; }

        /// <summary>
        /// Path to the directory that contains libR.so
        /// </summary>
        public string BinPath { get; private set; }

        public string DocPath { get; private set; }

        public string IncludePath { get; private set; }

        public string RShareDir { get; private set; }

        public string[] SiteLibraryDirs { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Name of the R interpreter</param>
        /// <param name="corePackage">Instance of the core interpreter package (r-base-core, microsoft-r-open-mro, etc)</param>
        /// <param name="fileSystem"></param>
        public RInterpreterInfo(string name, InstalledPackageInfo corePackage, string version, Version parsedVersion, IFileSystem fileSystem) {
            var files = corePackage.GetPackageFiles(fileSystem);
            Name = name;
            InstallPath = GetRInstallPath(files, fileSystem);
            _fileSystem = fileSystem;
            _packageFullVersion = version;
            Version = parsedVersion;

            BinPath = GetRLibPath();
            DocPath = GetRDocPath(files, fileSystem);
            IncludePath = GetIncludePath(files, fileSystem);
            RShareDir = GetRSharePath(files, fileSystem);
            SiteLibraryDirs = GetSiteLibraryDirs(corePackage, files, fileSystem);
        }

        public bool VerifyInstallation(ISupportedRVersionRange svr = null, IServiceContainer services = null) {
            var ui = services?.GetService<IUIService>();
            if (_isValid.HasValue) {
                return _isValid.Value;
            }

            _isValid = false;

            svr = svr ?? new SupportedRVersionRange();
            string libRPath = Path.Combine(BinPath, "libR.so");

            try {
                if (_fileSystem.DirectoryExists(InstallPath) && _fileSystem.DirectoryExists(BinPath) &&
                    _fileSystem.FileExists(libRPath)) {
                    if (Version != null) {
                        _isValid = svr.IsCompatibleVersion(Version);
                        if (!_isValid.Value) {
                            ui?.ShowMessage(
                                Resources.Error_UnsupportedRVersion.FormatInvariant(
                                Version.Major, Version.Minor, Version.Build, svr.MinMajorVersion, svr.MinMinorVersion, "*",
                                svr.MaxMajorVersion, svr.MaxMinorVersion, "*"), MessageButtons.OK);
                        }
                    } else {
                        // In linux there is no direct way to get version from binary. So assume valid version for a user provided
                        // interpreter path.
                        _isValid = true;
                    }
                } else {
                    ui?.ShowMessage(Resources.Error_CannotFindRBinariesFormat.FormatInvariant(InstallPath), MessageButtons.OK);
                }
            } catch (Exception ex) when (ex is IOException || ex is ArgumentException || ex is UnauthorizedAccessException) {
                ui?.ShowErrorMessage(Resources.Error_ExceptionAccessingPath.FormatInvariant(InstallPath, ex.Message));
            }

            return _isValid.Value;
        }

        private string GetRLibPath() {
            return Path.Combine(InstallPath, "lib");
        }

        public static RInterpreterInfo CreateFromPackage(InstalledPackageInfo package, string namePrefix, IFileSystem fs) {
            return new RInterpreterInfo($"{namePrefix} '{package.Version}'", package, package.Version, package.GetVersion(), fs);
        }

        private static string GetRInstallPath(IEnumerable<string> files, IFileSystem fs) {
            return GetPath(files, "/R/lib/libR.so", "/lib/libR.so", fs) ;
        }

        private static string GetRDocPath(IEnumerable<string> files, IFileSystem fs) {
            return GetPath(files, "/R/doc/html", "/html", fs);
        }

        private static string GetRSharePath(IEnumerable<string> files, IFileSystem fs) {
            return GetPath(files, "/R/share/R", "/R", fs);
        }

        private static string GetIncludePath(IEnumerable<string> files, IFileSystem fs) {
            return GetPath(files, "/R/include/R.h", "/R.h", fs);
        }

        private static string[] GetSiteLibraryDirs(InstalledPackageInfo package, IEnumerable<string> files, IFileSystem fs) {
            List<string> dirs = new List<string>();
            if (package.PackageName.ContainsIgnoreCase("r-base-core")) {
                string localSiteLibrary = "/usr/local/r/site-library";
                if (fs.DirectoryExists(localSiteLibrary)) {
                    dirs.Add(localSiteLibrary);
                }

                string siteLibrary = GetPath(files, "/R/site-library", "", fs);
                if (!string.IsNullOrWhiteSpace(siteLibrary)) {
                    dirs.Add(siteLibrary);
                }
            }

            string library = GetPath(files, "/R/library/base", "/base", fs);
            if (!string.IsNullOrWhiteSpace(library)) {
                dirs.Add(library);
            }

            return dirs.ToArray();
        }

        private static string GetPath(IEnumerable<string> files, string endPattern, string subPattern, IFileSystem fs) {
            var libFiles = files.Where(f => f.EndsWithIgnoreCase(endPattern));
            foreach (var f in libFiles) {
                string path = f.Substring(0, f.Length - subPattern.Length);
                if (fs.DirectoryExists(f) || fs.FileExists(f)) {
                    return path;
                }
            }
            return string.Empty;
        }
    }
}
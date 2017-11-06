// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI;

namespace Microsoft.R.Platform.Interpreters {
    internal abstract class UnixInterpreterInfo: IRInterpreterInfo {
        private bool? _isValid;

        protected IFileSystem FileSystem { get; }

        protected UnixInterpreterInfo(string name, Version version, IFileSystem fileSystem) {
            Name = name;
            Version = version;
            FileSystem = fileSystem;
        }

        public string Name { get; }
        public Version Version { get; }

        /// <summary>
        /// Path to the /R directory that contains libs, doc, etc
        /// </summary>
        public string InstallPath { get; protected set; }

        /// <summary>
        /// Path to the directory that contains libR.so
        /// </summary>
        public string BinPath { get; protected set; }

        /// <summary>
        /// Path to R.dll (Windows) or libR.dylib (MacOS)
        /// </summary>
        public string LibPath { get; protected set; }

        public abstract string LibName { get; }

        public string DocPath { get; protected set; }
        public string IncludePath { get; protected set; }
        public string RShareDir { get; protected set; }
        public string[] SiteLibraryDirs { get; protected set; }

        public bool VerifyInstallation(ISupportedRVersionRange svr = null, IServiceContainer services = null) {
            var ui = services?.GetService<IUIService>();
            if (_isValid.HasValue) {
                return _isValid.Value;
            }

            _isValid = false;

            svr = svr ?? new SupportedRVersionRange();
            try {
                if (FileSystem.DirectoryExists(InstallPath) && FileSystem.DirectoryExists(BinPath) &&
                    FileSystem.FileExists(Path.Combine(LibPath, LibName))) {
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

        protected string GetRLibPath() => Path.Combine(InstallPath, "lib");

        protected static string GetPath(IEnumerable<string> files, string endPattern, string subPattern, IFileSystem fs) {
            var libFiles = files.Where(f => f.EndsWithIgnoreCase(endPattern));
            foreach (var f in libFiles) {
                var path = f.Substring(0, f.Length - subPattern.Length);
                if (fs.DirectoryExists(f) || fs.FileExists(f)) {
                    return path;
                }
            }
            return string.Empty;
        }

    }
}

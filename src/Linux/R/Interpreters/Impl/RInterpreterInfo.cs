// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.UI;

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
        public string BinPath { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Name of the R interpreter</param>
        /// <param name="path">Path to the /R folder</param>
        /// <param name="fileSystem"></param>
        public RInterpreterInfo(string name, string path, string version, Version parsedVersion, IFileSystem fileSystem) {
            Name = name;
            InstallPath = path;
            BinPath = GetRLibPath();
            _fileSystem = fileSystem;
            _packageFullVersion = version;
            Version = parsedVersion;
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
    }
}
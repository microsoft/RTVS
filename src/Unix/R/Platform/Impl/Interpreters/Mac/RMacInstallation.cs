// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.R.Platform.IO;

namespace Microsoft.R.Platform.Interpreters.Mac {
    public sealed class RMacInstallation : IRInstallationService {
        private const string RootPath = "/Library/Frameworks/R.framework/Versions/";
        private readonly IFileSystem _fileSystem;

        public RMacInstallation() :
            this(new UnixFileSystem()) { }

        public RMacInstallation(IFileSystem fileSystem) {
            _fileSystem = fileSystem;
        }

        public IRInterpreterInfo CreateInfo(string name, string path) {
            var version = VersionFromPath(path, out var versionString);
            return version != null ? new RMacInterpreterInfo("R " + versionString, versionString, version, _fileSystem) : null;
        }

        public IEnumerable<IRInterpreterInfo> GetCompatibleEngines(ISupportedRVersionRange svl = null) {
            var interpreters = new List<IRInterpreterInfo>();
            interpreters.AddRange(GetInstalledCranR(svl));
            return interpreters;
        }

        private IEnumerable<IRInterpreterInfo> GetInstalledCranR(ISupportedRVersionRange svl) {
            var rFrameworkPath = Path.Combine("/Library/Frameworks/R.framework/Versions");

            foreach (var path in _fileSystem.GetDirectories(rFrameworkPath)) {
                var version = VersionFromPath(path, out var versionString);
                if (version != null && svl.IsCompatibleVersion(version)) {
                    yield return new RMacInterpreterInfo("R " + versionString, versionString, version, _fileSystem);
                }
            }
        }

        private static Version VersionFromPath(string path, out string versionString) {
            versionString = null;
            if (!path.StartsWithOrdinal(RootPath)) {
                return null;
            }
            var resPath = path.Substring(RootPath.Length, path.Length - RootPath.Length);
            var index = resPath.IndexOf("/Resources");
            versionString = index > 0 ? resPath.Substring(0, index) : resPath;
            return Version.TryParse(versionString, out var version) ? version : null;
        }
    }
}
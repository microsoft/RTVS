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
            this(new UnixFileSystem()) {
        }

        public RMacInstallation(IFileSystem fileSystem) {
            _fileSystem = fileSystem;
        }

        public IRInterpreterInfo CreateInfo(string name, string path) {
            if (path.StartsWithOrdinal(RootPath)) { }
            var resPath = path.Substring(RootPath.Length, path.Length - RootPath.Length);
            var index = resPath.IndexOf("/Resources");
            if(index > 0) {
                var versionString = resPath.Substring(0, index);
                if (Version.TryParse(versionString, out var version)) {
                    return new RMacInterpreterInfo("R " + versionString, versionString, version, _fileSystem);
                }
            }
            return null;
        }

        public IEnumerable<IRInterpreterInfo> GetCompatibleEngines(ISupportedRVersionRange svl = null) {
            var interpreters = new List<IRInterpreterInfo>();
            interpreters.AddRange(GetInstalledCranR(svl));
            return interpreters;
        }

        private IEnumerable<IRInterpreterInfo> GetInstalledCranR(ISupportedRVersionRange svl) {
            var rFrameworkPath = Path.Combine("/Library/Frameworks/R.framework/Versions");

            foreach (var dir in _fileSystem.GetDirectories(rFrameworkPath)) {
                if (Version.TryParse(dir, out var version) && svl.IsCompatibleVersion(version)) {
                    yield return new RMacInterpreterInfo("R " + dir, dir, version, _fileSystem);
                }
            }
        }
    }
}

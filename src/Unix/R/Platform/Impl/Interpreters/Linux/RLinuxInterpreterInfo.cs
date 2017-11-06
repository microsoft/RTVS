// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.R.Platform.OS.Linux;

namespace Microsoft.R.Platform.Interpreters.Linux {
    internal sealed class RLinuxInterpreterInfo : UnixInterpreterInfo, IRInterpreterInfo {
        private readonly string _packageFullVersion;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the R interpreter</param>
        /// <param name="corePackage">Instance of the core interpreter package 
        /// (r-base-core, microsoft-r-open-mro, etc)</param>
        /// <param name="fileSystem"></param>
        public RLinuxInterpreterInfo(string name, InstalledPackageInfo corePackage, string version, Version parsedVersion, IFileSystem fileSystem) : 
            base(name, parsedVersion, fileSystem) {
            var files = corePackage.GetPackageFiles(fileSystem);
            InstallPath = GetRInstallPath(files, fileSystem);
            _packageFullVersion = version;

            BinPath = GetRLibPath();
            DocPath = GetRDocPath(files, fileSystem);
            IncludePath = GetIncludePath(files, fileSystem);
            RShareDir = GetRSharePath(files, fileSystem);
            SiteLibraryDirs = GetSiteLibraryDirs(corePackage, files, fileSystem);
        }

        /// <summary>
        /// Name of the R dynamic library such as  R.dll (Windows) or libR.dylib (MacOS)
        /// </summary>
        public override string LibName => "LibR.so";

        public static RLinuxInterpreterInfo CreateFromPackage(InstalledPackageInfo package, string namePrefix, IFileSystem fs) 
            => new RLinuxInterpreterInfo($"{namePrefix} '{package.Version}'", package, package.Version, package.GetVersion(), fs);

        private static string GetRInstallPath(IEnumerable<string> files, IFileSystem fs) 
            => GetPath(files, "/R/lib/libR.so", "/lib/libR.so", fs);

        private static string GetRDocPath(IEnumerable<string> files, IFileSystem fs) 
            => GetPath(files, "/R/doc/html", "/html", fs);

        private static string GetRSharePath(IEnumerable<string> files, IFileSystem fs) 
            => GetPath(files, "/R/share/R", "/R", fs);

        private static string GetIncludePath(IEnumerable<string> files, IFileSystem fs) 
            => GetPath(files, "/R/include/R.h", "/R.h", fs);

        private static string[] GetSiteLibraryDirs(InstalledPackageInfo package, IEnumerable<string> files, IFileSystem fs) {
            var dirs = new List<string>();
            if (package.PackageName.ContainsIgnoreCase("r-base-core")) {
                var localSiteLibrary = "/usr/local/r/site-library";
                if (fs.DirectoryExists(localSiteLibrary)) {
                    dirs.Add(localSiteLibrary);
                }

                var siteLibrary = GetPath(files, "/R/site-library", "", fs);
                if (!string.IsNullOrWhiteSpace(siteLibrary)) {
                    dirs.Add(siteLibrary);
                }
            }

            var library = GetPath(files, "/R/library/base", "/base", fs);
            if (!string.IsNullOrWhiteSpace(library)) {
                dirs.Add(library);
            }

            return dirs.ToArray();
        }
    }
}
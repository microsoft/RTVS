// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.R.Common.Core.Linux;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.R.Interpreters
{
    public sealed class RInstallation : IRInstallationService
    {

        private const string _mroDirectory = "/usr/lib64/microsoft-r";
        private const string _mroName = "Microsoft R Open";
        private readonly IFileSystem _fileSystem;

        public RInstallation() :
            this(new FileSystem())
        { }

        public RInstallation(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public IRInterpreterInfo CreateInfo(string name, string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IRInterpreterInfo> GetCompatibleEngines(ISupportedRVersionRange svl = null)
        {
            var packagesInfo = InstalledPackageInfo.GetPackages(_fileSystem);
            List<IRInterpreterInfo> interpreters = new List<IRInterpreterInfo>();

            interpreters.AddRange(GetInstalledMRO(packagesInfo, svl));
            interpreters.AddRange(GetInstalledCranR(packagesInfo, svl));
            return interpreters;
        }

        private IEnumerable<IRInterpreterInfo> GetInstalledMRO(IEnumerable<InstalledPackageInfo> packagesInfo, ISupportedRVersionRange svl)
        {
            var list = new List<IRInterpreterInfo>();
            var selectedPackages = packagesInfo.Where(p => p.PackageName.StartsWithIgnoreCase("microsoft-r-open-mro") && svl.IsCompatibleVersion(p.GetVersion()));
            foreach (var package in selectedPackages)
            {
                var files = package.GetPackageFiles(_fileSystem);
                string libRPath = GetLibRPath(files, _fileSystem);
                list.Add(new RInterpreterInfo($"Microsoft R Open '{package.Version}'", Path.GetDirectoryName(libRPath), _fileSystem));
            }

            return list;
        }

        private IEnumerable<IRInterpreterInfo> GetInstalledCranR(IEnumerable<InstalledPackageInfo> packagesInfo, ISupportedRVersionRange svl)
        {
            var list = new List<IRInterpreterInfo>();
            var selectedPackages = packagesInfo.Where(p => p.PackageName.StartsWithIgnoreCase("microsoft-r-open-mro") && svl.IsCompatibleVersion(p.GetVersion()));
            foreach (var package in selectedPackages)
            {
                var files = package.GetPackageFiles(_fileSystem);
                string libRPath = GetLibRPath(files, _fileSystem);
                list.Add(new RInterpreterInfo($"R '{package.Version}'", Path.GetDirectoryName(libRPath), _fileSystem));
            }

            return list;
        }



        private string GetLibRPath(IEnumerable<string> files, IFileSystem fs)
        {
            var libFiles = files.Where(f => f.EndsWithIgnoreCase("/R/lib/libR.so"));
            foreach (var f in libFiles)
            {
                if (fs.FileExists(f))
                {
                    return f;
                }
            }
            return string.Empty;
        }
    }
}

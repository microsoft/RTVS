// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.IO;
using System.IO;

namespace Microsoft.R.Interpreters
{
    public sealed class RInterpreterInfo : IRInterpreterInfo
    {
        private readonly IFileSystem _fileSystem;

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
        public RInterpreterInfo(string name, string path, IFileSystem fileSystem)
        {
            Name = name;
            InstallPath = path;
            BinPath = GetRLibPath();
            _fileSystem = fileSystem;
        }

        public bool VerifyInstallation(ISupportedRVersionRange svr = null, IServiceContainer services = null)
        {
            string libRPath = Path.Combine(BinPath, "libR.so");
            return _fileSystem.DirectoryExists(InstallPath) && _fileSystem.DirectoryExists(BinPath) && _fileSystem.DirectoryExists(libRPath);
        }

        private string GetRLibPath()
        {
            return Path.Combine(InstallPath, "lib");
        }
    }
}
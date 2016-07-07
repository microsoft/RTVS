// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.IO;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Functions;

namespace Microsoft.R.Support.Help.Packages {
    /// <summary>
    /// Base class for package collections
    /// </summary>
    public class PackageCollection : IPackageCollection {
        private readonly IFunctionIndex _functionIndex;
        public string InstallPath { get; }

        public IEnumerable<IPackageInfo> Packages {
            get {
                try {
                    string libraryPath = this.InstallPath;
                    if (!string.IsNullOrEmpty(libraryPath)) {
                        return new PackageEnumeration(_functionIndex, libraryPath);
                    }
                } catch (IOException) { }

                return new IPackageInfo[0];
            }
        }

        protected PackageCollection(IFunctionIndex functionIndex, string installPath) {
            _functionIndex = functionIndex;
            InstallPath = installPath;
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Support.Help.Definitions;

namespace Microsoft.R.Support.Help.Packages {
    /// <summary>
    /// Index of packages available from the R engine. Collection 
    /// of packages installed by the engine is furhished by a static
    /// provider while list of the user-installed packages and 
    /// list of per-project packages (when PackRat is used) 
    /// are supplied by the providers exported via MEF. 
    /// </summary>
    [Export(typeof(IPackageIndex))]
    public class PackageIndex : IPackageIndex {
        private readonly ICoreShell _shell;
        private readonly Dictionary<string, IPackageInfo> _packages = new Dictionary<string, IPackageInfo>();
        private readonly Lazy<IFunctionIndex> _functionIndexLazy;

        private IEnumerable<Lazy<IPackageCollection>> _collections;
        private IPackageCollection _basePackages;

        [ImportingConstructor]
        public PackageIndex(ICoreShell shell) {
            _shell = shell;
            _functionIndexLazy = Lazy.Create(() => _shell.ExportProvider.GetExportedValue<IFunctionIndex>());
        }

        /// <summary>
        /// Collection or packages installed with the R engine.
        /// </summary>
        public IEnumerable<IPackageInfo> BasePackages {
            get {
                if (_basePackages == null)
                    _basePackages = new BasePackagesCollection(_shell.ExportProvider.GetExportedValue<IFunctionIndex>());

                return _basePackages.Packages;
            }
        }

        /// <summary>
        /// Collection of all packages (base, user and project-specific)
        /// </summary>
        public IReadOnlyList<IPackageInfo> Packages {
            get {
                if (_collections == null)
                    _collections = _shell.ExportProvider.GetExports<IPackageCollection>();

                if (_collections != null) {
                    List<IPackageInfo> packages = new List<IPackageInfo>();

                    foreach (Lazy<IPackageCollection> collection in _collections) {
                        packages.AddRange(collection.Value.Packages);

                        foreach (IPackageInfo p in collection.Value.Packages) {
                            _packages[p.Name.ToLowerInvariant()] = p;
                        }
                    }

                    packages.AddRange(BasePackages);
                    return packages;
                }

                return new List<IPackageInfo>();
            }
        }

        /// <summary>
        /// Collection of all packages (base, user and project-specific)
        /// </summary>
        public IPackageInfo GetPackageByName(string packageName) {
            // Strip quotes, if any
            packageName = packageName.Replace("\'", string.Empty).Replace("\"", string.Empty).Trim();
            packageName = packageName.ToLowerInvariant();

            var package = BasePackages.FirstOrDefault(p => p.Name.EqualsIgnoreCase(packageName));
            if (package == null) {
                _packages.TryGetValue(packageName, out package);
            }

            if (package == null) {
                package = TryFindPackageNew(packageName);
            }

            return package;
        }

        private IPackageInfo TryFindPackageNew(string packageName) {
            foreach (Lazy<IPackageCollection> collection in _collections) {
                IPackageInfo package = collection.Value.Packages.FirstOrDefault(p => p.Name.EqualsIgnoreCase(packageName));
                if (package != null) {
                    _packages[package.Name.ToLowerInvariant()] = package;
                    _functionIndexLazy.Value.BuildIndexForPackage(package);
                    return package;
                }
            }

            return null;
        }
    }
}

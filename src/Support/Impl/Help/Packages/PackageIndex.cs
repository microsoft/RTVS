using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Support.Help.Definitions;

namespace Microsoft.R.Support.Help.Packages
{
    /// <summary>
    /// Index of packages available from the R engine. Collection 
    /// of packages installed by the engine is furhished by a static
    /// provider while list of the user-installed packages and 
    /// list of per-project packages (when PackRat is used) 
    /// are supplied by the providers exported via MEF. 
    /// </summary>
    public static class PackageIndex
    {
        private static IEnumerable<Lazy<IPackageCollection>> _collections;
        private static IPackageCollection _basePackages;
        private static Dictionary<string, IPackageInfo> _packages = new Dictionary<string, IPackageInfo>();
        /// <summary>
        /// Collection or packages installed with the R engine.
        /// </summary>
        public static IEnumerable<IPackageInfo> BasePackages
        {
            get
            {
                if (_basePackages == null)
                    _basePackages = new BasePackagesCollection();

                return _basePackages.Packages;
            }
        }

        /// <summary>
        /// Collection of all packages (base, user and project-specific)
        /// </summary>
        public static IReadOnlyList<IPackageInfo> Packages
        {
            get
            {
                if (_collections == null)
                    _collections = EditorShell.ExportProvider.GetExports<IPackageCollection>();

                if (_collections != null)
                {
                    List<IPackageInfo> packages = new List<IPackageInfo>();

                    foreach (Lazy<IPackageCollection> collection in _collections)
                    {
                        packages.AddRange(collection.Value.Packages);

                        foreach (IPackageInfo p in collection.Value.Packages)
                        {
                            _packages[p.Name] = p;
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
        public static IPackageInfo GetPackageByName(string packageName)
        {
            IPackageInfo package;
            _packages.TryGetValue(packageName, out package);

            return package;
        }
    }
}

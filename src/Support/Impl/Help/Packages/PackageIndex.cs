using System;
using System.Collections.Generic;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Support.Help.Definitions;

namespace Microsoft.R.Support.Help.Packages
{
    public static class PackageIndex
    {
        private static IEnumerable<Lazy<IPackageCollection>> _collections;
        private static IPackageCollection _basePackages;

        public static IEnumerable<IPackageInfo> BasePackages
        {
            get
            {
                if (_basePackages != null)
                    _basePackages = new BasePackagesCollection();

                return _basePackages.Packages;
            }
        }

        public static IReadOnlyList<IPackageInfo> Packages
        {
            get
            {
                if (_collections == null)
                    _collections = EditorShell.ExportProvider.GetExports<IPackageCollection>();

                if(_collections != null)
                {
                    List<IPackageInfo> packages = new List<IPackageInfo>();

                    foreach(var collection in _collections)
                    {
                        packages.AddRange(collection.Value.Packages);
                    }

                    packages.AddRange(BasePackages);
                    return packages;
                }

                return new List<IPackageInfo>();
            }
        }
    }
}

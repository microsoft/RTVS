using System.Collections.Generic;
using System.IO;
using Microsoft.R.Support.Help.Definitions;

namespace Microsoft.R.Support.Help.Packages
{
    public abstract class PackagesCollection : IPackageCollection
    {
        public abstract string InstallPath { get; }

        public IEnumerable<IPackageInfo> Packages
        {
            get
            {
                try
                {
                    string libraryPath = this.InstallPath;
                    if (string.IsNullOrEmpty(libraryPath))
                    {
                        return new PackageEnumeration(libraryPath, isBase: true);
                    }
                }
                catch (IOException) { }

                return new IPackageInfo[0];
            }
        }
    }
}

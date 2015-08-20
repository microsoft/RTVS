using System.Collections.Generic;
using System.IO;
using Microsoft.R.Support.Help.Definitions;

namespace Microsoft.R.Support.Help.Packages
{
    /// <summary>
    /// Base class for package collections
    /// </summary>
    public class PackageCollection : IPackageCollection
    {
        public string InstallPath { get; private set; }

        public IEnumerable<IPackageInfo> Packages
        {
            get
            {
                try
                {
                    string libraryPath = this.InstallPath;
                    if (!string.IsNullOrEmpty(libraryPath))
                    {
                        return new PackageEnumeration(libraryPath);
                    }
                }
                catch (IOException) { }

                return new IPackageInfo[0];
            }
        }

        protected PackageCollection(string installPath)
        {
            InstallPath = installPath;
        }
    }
}

using System.Collections.Generic;
using System.IO;
using Microsoft.R.Support.Help.Definitions;

namespace Microsoft.R.Support.Help.Packages
{
    /// <summary>
    /// Base class for package collections
    /// </summary>
    public abstract class PackageCollection : IPackageCollection
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
                        return new PackageEnumeration(libraryPath);
                    }
                }
                catch (IOException) { }

                return new IPackageInfo[0];
            }
        }
    }
}

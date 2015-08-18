using System.Collections.Generic;

namespace Microsoft.R.Support.Help.Definitions
{
    public interface IPackageCollection
    {
        /// <summary>
        /// Path to base R packages. Typically ~/Program Files/R/[version]/library
        /// </summary>
        string InstallPath { get; }

        /// <summary>
        /// Enumerates base packages
        /// </summary>
        IEnumerable<IPackageInfo> Packages { get; }
    }
}

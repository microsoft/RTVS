using System.Collections.Generic;
using Microsoft.R.Support.Utility.Definitions;

namespace Microsoft.R.Support.Help.Definitions
{
    public interface IPackageInfo : INamedItemInfo, IAsyncDataSource<string>
    {
        /// <summary>
        /// Package install path
        /// </summary>
        string InstallPath { get; }

        /// <summary>
        /// Determines if package is part of the R engine
        /// </summary>
        bool IsBase { get; }

        /// <summary>
        /// List of functions in the package
        /// </summary>
        IReadOnlyCollection<INamedItemInfo> Functions { get; }
    }
}

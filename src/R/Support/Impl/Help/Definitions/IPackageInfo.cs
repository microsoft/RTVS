// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.R.Support.Help.Definitions {
    public interface IPackageInfo : INamedItemInfo {
        /// <summary>
        /// Package install path
        /// </summary>
        string InstallPath { get; }

        /// <summary>
        /// List of functions in the package
        /// </summary>
        IReadOnlyCollection<INamedItemInfo> Functions { get; }
    }
}

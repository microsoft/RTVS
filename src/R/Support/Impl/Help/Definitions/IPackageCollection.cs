// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.R.Support.Help.Definitions {
    /// <summary>
    /// Describes collection or R packages. 
    /// Typically implementation is exported via MEF
    /// since there multiple collections exist.
    /// </summary>
    public interface IPackageCollection {
        /// <summary>
        /// Path to base R packages. 
        /// Typically ~/Program Files/R/[version]/library
        /// </summary>
        string InstallPath { get; }

        /// <summary>
        /// Enumerates base packages
        /// </summary>
        IEnumerable<IPackageInfo> Packages { get; }
    }
}

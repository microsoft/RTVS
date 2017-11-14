// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Components.PackageManager;

namespace Microsoft.R.Editor.Functions {
    public interface IPackageIndex: IPackageInstallationNotifications, IDisposable {
        /// <summary>
        /// Creates index of packages available in the provided R session
        /// </summary>
        Task BuildIndexAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Writes index cache to disk
        /// </summary>
        void WriteToDisk();

        /// <summary>
        /// Returns collection of packages in the current 
        /// </summary>
        IEnumerable<IPackageInfo> Packages { get; }

        /// <summary>
        /// Retrieves R package information by name. If package is not in the index,
        /// attempts to locate the package in the current R session.
        /// </summary>
        Task<IPackageInfo> GetPackageInfoAsync(string packageName, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Retrieves information on multilple R packages. If one of the packages 
        /// is not in the index, attempts to locate the package in the current R session.
        /// </summary>
        Task<IEnumerable<IPackageInfo>> GetPackagesInfoAsync(IEnumerable<string> packageNames, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Path to the cache.
        /// </summary>
        string CacheFolderPath { get; }

        /// <summary>
        /// Clears in-memory function and package information cache.
        /// </summary>
        void ClearCache();
    }
}
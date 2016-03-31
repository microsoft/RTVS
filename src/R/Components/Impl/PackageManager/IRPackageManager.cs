// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.R.Components.PackageManager.Model;

namespace Microsoft.R.Components.PackageManager {
    public interface IRPackageManager : IDisposable {
        IRPackageManagerVisualComponent VisualComponent { get; }

        IRPackageManagerVisualComponent GetOrCreateVisualComponent(IRPackageManagerVisualComponentContainerFactory visualComponentContainerFactory, int instanceId = 0);

        /// <summary>
        /// Get the list of packages installed in the library folders set for
        /// this session ie. in .libPaths().
        /// </summary>
        /// <returns>List of packages.</returns>
        /// <exception cref="RPackageManagerException">
        /// The package list couldn't be retrieved from the session.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// </exception>
        Task<IReadOnlyList<RPackage>> GetInstalledPackagesAsync();

        /// <summary>
        /// Get the list of packages that are available from the repositories
        /// set for this session ie. in getOption('repos').
        /// </summary>
        /// <returns>
        /// List of packages. Note that several fields will not be populated,
        /// you need to call <see cref="GetAdditionalPackageInfoAsync(RPackage)"/>
        /// for each package to get fill in the missing fields.
        /// </returns>
        /// <exception cref="RPackageManagerException">
        /// The package list couldn't be retrieved from the session.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// </exception>
        Task<IReadOnlyList<RPackage>> GetAvailablePackagesAsync();

        /// <summary>
        /// Get additional data for a package from its repository web site.
        /// </summary>
        /// <param name="pkg">
        /// Package to populate with data. The <see cref="RPackage.Repository"/>
        /// and <see cref="RPackage.Package"/> fields must be set prior to
        /// calling this method. Any fields that are already filled in will not
        /// be overwritten.
        /// </param>
        /// <exception cref="RPackageManagerException">
        /// The web page for the package couldn't be downloaded.
        /// </exception>
        Task GetAdditionalPackageInfoAsync(RPackage pkg);

        /// <summary>
        /// Install a package by sending install.packages() to the REPL.
        /// </summary>
        /// <param name="name">Package name.</param>
        /// <param name="libraryPath">
        /// Optional library path (in any format). Pass null to use the default
        /// for the session ie. the first one in .libPaths().
        /// </param>
        void InstallPackage(string name, string libraryPath);

        /// <summary>
        /// Uninstall a package by sending remove.packages() to the REPL.
        /// </summary>
        /// <param name="name">Package name.</param>
        /// <param name="libraryPath">
        /// Optional library path (in any format) where the package is installed.
        /// Pass null to use the defaults for the session ie. in .libPaths().
        /// </param>
        void UninstallPackage(string name, string libraryPath);

        /// <summary>
        /// Load a package by sending library() to the REPL.
        /// </summary>
        /// <param name="name">Package name.</param>
        /// <param name="libraryPath">
        /// Optional library path (in any format). Pass null to use the defaults
        /// for the session ie. in .libPaths().
        /// </param>
        void LoadPackage(string name, string libraryPath);

        /// <summary>
        /// Unload a package by sending detach() to the REPL.
        /// </summary>
        /// <param name="name">Package name.</param>
        void UnloadPackage(string name);

        /// <summary>
        /// Package names that are currently loaded.
        /// </summary>
        /// <returns>Array of package names.</returns>
        /// <exception cref="RPackageManagerException">
        /// The package list couldn't be retrieved from the session.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// </exception>
        Task<string[]> GetLoadedPackagesAsync();

        /// <summary>
        /// Paths of library folders, as returned .libPaths().
        /// </summary>
        /// <returns>Array of paths, in R format (ie. c:/libs/lib1).</returns>
        /// <exception cref="RPackageManagerException">
        /// The library list couldn't be retrieved from the session.
        /// </exception>
        /// <exception cref="OperationCanceledException">
        /// </exception>
        Task<string[]> GetLibraryPathsAsync();
    }
}
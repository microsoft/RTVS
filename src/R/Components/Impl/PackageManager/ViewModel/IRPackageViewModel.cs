// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.R.Components.PackageManager.Model;

namespace Microsoft.R.Components.PackageManager.ViewModel {
    public interface IRPackageViewModel {
        string Name { get; }
        string Title { get; }
        string Description { get; }
        string LatestVersion { get; }
        string InstalledVersion { get; }
        string Authors { get; }
        string License { get; }
        ICollection<string> Urls { get; }
        bool NeedsCompilation { get; }
        string LibraryPath { get; }
        string RepositoryText { get; }
        Uri RepositoryUri { get; }
        string Built { get; }

        string Depends { get; }
        string Imports { get; }
        string Suggests { get; }

        bool IsInstalled { get; set; }
        bool IsLoaded { get; set; }
        bool IsUpdateAvailable { get; }
        bool CanBeUninstalled { get; set; }
        bool HasDetails { get; }
        bool IsChecked { get; set; }
        bool IsChanging { get; set; }
        bool IsRemoteSession { get; }

        void AddDetails(RPackage package, bool isInstalled);
        void UpdateAvailablePackageDetails(RPackage package);
        Task InstallAsync();
        Task UninstallAsync();
        Task UpdateAsync();
        string AccessibleDescription { get; }
    }
}

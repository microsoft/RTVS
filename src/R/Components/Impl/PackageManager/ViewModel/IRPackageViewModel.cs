// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Components.PackageManager.Model;

namespace Microsoft.R.Components.PackageManager.ViewModel {
    public interface IRPackageViewModel {
        string Name { get; }
        string Title { get; }
        RPackageVersion LatestVersion { get; }
        RPackageVersion InstalledVersion { get; }
        string Dependencies { get; }
        string License { get; }

        bool IsInstalled { get; }
        bool IsUpdateAvailable { get; }
        bool IsSelected { get; set; }
    }
}

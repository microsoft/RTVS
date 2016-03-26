// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;

namespace Microsoft.R.Components.PackageManager.ViewModel {
    public interface IRPackageManagerViewModel {
        ObservableCollection<object> Items { get; } 
        IRPackageViewModel SelectedPackage { get; }

        void SwitchToAvailablePackages();
        void SwitchToInstalledPackages();
        void SwitchToLoadedPackages();
        void ReloadItems();
    }
}

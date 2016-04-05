// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using Microsoft.R.Components.Search;

namespace Microsoft.R.Components.PackageManager.ViewModel {
    public interface IRPackageManagerViewModel : ISearchHandler {
        ReadOnlyObservableCollection<object> Items { get; } 
        IRPackageViewModel SelectedPackage { get; }
        bool IsLoading { get; }
        bool ShowPackageManagerDisclaimer { get; set; }

        void SwitchToAvailablePackages();
        void SwitchToInstalledPackages();
        void SwitchToLoadedPackages();
        void ReloadItems();
        void SelectPackage(IRPackageViewModel package);
        void Install(IRPackageViewModel package);
        void Uninstall(IRPackageViewModel package);
    }
}

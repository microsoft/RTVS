// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.R.Components.Search;

namespace Microsoft.R.Components.PackageManager.ViewModel {
    public interface IRPackageManagerViewModel : ISearchHandler, IDisposable {
        ReadOnlyObservableCollection<object> Items { get; } 
        IRPackageViewModel SelectedPackage { get; }
        bool IsLoading { get; }
        bool ShowPackageManagerDisclaimer { get; set; }

        string FirstError { get; }
        bool HasMultipleErrors { get; }

        Task SwitchToAvailablePackagesAsync();
        Task SwitchToInstalledPackagesAsync();
        Task SwitchToLoadedPackagesAsync();
        Task ReloadCurrentTabAsync();
        void SelectPackage(IRPackageViewModel package);
        Task InstallAsync(IRPackageViewModel package);
        Task UpdateAsync(IRPackageViewModel package);
        Task UninstallAsync(IRPackageViewModel package);
        Task LoadAsync(IRPackageViewModel package);
        Task UnloadAsync(IRPackageViewModel package);
        Task DefaultActionAsync();
        void DismissErrorMessage();
        void DismissAllErrorMessages();
    }
}

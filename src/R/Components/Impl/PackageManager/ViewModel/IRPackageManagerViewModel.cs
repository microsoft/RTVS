// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.R.Components.Search;

namespace Microsoft.R.Components.PackageManager.ViewModel {
    public interface IRPackageManagerViewModel : ISearchHandler, IDisposable {
        ReadOnlyObservableCollection<object> Items { get; } 
        IRPackageViewModel SelectedPackage { get; }
        bool IsLoading { get; }
        bool ShowPackageManagerDisclaimer { get; set; }

        bool HasErrors { get; }
        bool IsRemoteSession { get; }

        Task SwitchToAvailablePackagesAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task SwitchToInstalledPackagesAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task SwitchToLoadedPackagesAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task ReloadCurrentTabAsync(CancellationToken cancellationToken = default(CancellationToken));
        void SelectPackage(IRPackageViewModel package);
        Task InstallAsync(IRPackageViewModel package, CancellationToken cancellationToken = default(CancellationToken));
        Task UpdateAsync(IRPackageViewModel package, CancellationToken cancellationToken = default(CancellationToken));
        Task UninstallAsync(IRPackageViewModel package, CancellationToken cancellationToken = default(CancellationToken));
        Task LoadAsync(IRPackageViewModel package, CancellationToken cancellationToken = default(CancellationToken));
        Task UnloadAsync(IRPackageViewModel package, CancellationToken cancellationToken = default(CancellationToken));
        Task DefaultActionAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}

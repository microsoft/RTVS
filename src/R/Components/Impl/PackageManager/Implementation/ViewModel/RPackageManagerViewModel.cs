// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Wpf;
using Microsoft.Common.Wpf.Collections;
using Microsoft.R.Components.PackageManager.Model;
using Microsoft.R.Components.PackageManager.ViewModel;

namespace Microsoft.R.Components.PackageManager.Implementation.ViewModel {
    internal class RPackageManagerViewModel : BindableBase, IRPackageManagerViewModel {
        private readonly IRPackageManager _packageManager;
        private readonly ICoreShell _coreShell;
        private readonly BinaryAsyncLock _availableAndInstalledLock;
        private readonly BatchObservableCollection<object> _items;

        private volatile IList<IRPackageViewModel> _availablePackages;
        private volatile IList<IRPackageViewModel> _installedPackages;
        private volatile IList<IRPackageViewModel> _loadedPackages;

        public RPackageManagerViewModel(IRPackageManager packageManager, ICoreShell coreShell) {
            _packageManager = packageManager;
            _coreShell = coreShell;
            _availableAndInstalledLock = new BinaryAsyncLock();
            _items = new BatchObservableCollection<object>();
            Items = new ReadOnlyObservableCollection<object>(_items);
        }

        public ReadOnlyObservableCollection<object> Items { get; }

        public IRPackageViewModel SelectedPackage {
            get { return _selectedPackage; }
            private set { SetProperty(ref _selectedPackage, value); }
        }

        public bool IsLoading {
            get { return _isLoading; }
            private set { SetProperty(ref _isLoading, value); }
        }

        private SelectedTab _selectedTab;
        private bool _isLoading;
        private IRPackageViewModel _selectedPackage;
        private volatile string _searchString;

        public void SwitchToAvailablePackages() {
            _coreShell.AssertIsOnMainThread();
            if (_selectedTab == SelectedTab.AvailablePackages && _availablePackages != null) {
                return;
            }

            _selectedTab = SelectedTab.AvailablePackages;
            DispatchOnMainThreadAsync(SwitchToAvailablePackagesAsync);
        }

        public void SwitchToInstalledPackages() {
            _coreShell.AssertIsOnMainThread();
            if (_selectedTab == SelectedTab.InstalledPackages) {
                return;
            }

            _selectedTab = SelectedTab.InstalledPackages;
            DispatchOnMainThreadAsync(SwitchToInstalledPackagesAsync);
        }

        public void SwitchToLoadedPackages() {
            _coreShell.AssertIsOnMainThread();
            if (_selectedTab == SelectedTab.LoadedPackages) {
                return;
            }

            _selectedTab = SelectedTab.LoadedPackages;
            ReloadItems();
        }

        public void ReloadItems() {
            _coreShell.AssertIsOnMainThread();
            switch (_selectedTab) {
                case SelectedTab.AvailablePackages:
                    _availableAndInstalledLock.Reset();
                    //LoadAvailablePackages();
                    break;
                case SelectedTab.InstalledPackages:
                    //LoadInstalledPackages();
                    break;
                case SelectedTab.LoadedPackages:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SelectPackage(IRPackageViewModel package) {
            _coreShell.AssertIsOnMainThread();
            if (package == _selectedPackage) {
                return;
            }

            SelectedPackage = package;
            if (package == null) {
                return;
            }

            if (!package.HasDetails) {
                DispatchOnMainThreadAsync(() => AddPackageDetailsAsync(package));
            }
        }

        private async Task AddPackageDetailsAsync(IRPackageViewModel package) {
            _coreShell.AssertIsOnMainThread();
            var details = await GetAdditionalPackageInfoAsync(package);
            package.AddDetails(details, false);
        }

        private async Task<RPackage> GetAdditionalPackageInfoAsync(IRPackageViewModel package) {
            await TaskUtilities.SwitchToBackgroundThread();
            return await _packageManager.GetAdditionalPackageInfoAsync(package.Name, package.Repository);
        }

        private async Task SwitchToAvailablePackagesAsync() {
            _coreShell.AssertIsOnMainThread();
            await EnsureInstalledAndAvailablePackagesLoadedAsync();
            if (_availablePackages == null) {
                return;
            }
            _items.ReplaceWith(_availablePackages);
        }

        private async Task SwitchToInstalledPackagesAsync() {
            _coreShell.AssertIsOnMainThread();
            await EnsureInstalledAndAvailablePackagesLoadedAsync();
            if (_installedPackages == null) {
                return;
            }
            _items.ReplaceWith(_installedPackages);
        }

        private async Task EnsureInstalledAndAvailablePackagesLoadedAsync() {
            var areLoaded = await _availableAndInstalledLock.WaitAsync();
            if (!areLoaded) {
                IsLoading = true;
                try {
                    await LoadInstalledAndAvailablePackagesAsync();
                } finally {
                    IsLoading = false;
                    _availableAndInstalledLock.Release();
                }
            }
        }

        private async Task LoadInstalledAndAvailablePackagesAsync() {
            await TaskUtilities.SwitchToBackgroundThread();

            var availablePackages = await _packageManager.GetAvailablePackagesAsync();
            var installedPackages = await _packageManager.GetInstalledPackagesAsync();

            var vmAvailablePackages = availablePackages.Select(RPackageViewModel.CreateAvailable).ToDictionary(p => p.Name);
            var vmInstalledPackages = new List<IRPackageViewModel>();
            foreach (var installedPackage in installedPackages) {
                RPackageViewModel vmPackage;
                if (vmAvailablePackages.TryGetValue(installedPackage.Package, out vmPackage)) {
                    vmPackage.AddDetails(installedPackage, true);
                    vmInstalledPackages.Add(vmPackage);
                } else {
                    vmInstalledPackages.Add(RPackageViewModel.CreateInstalled(installedPackage));
                }
            }

            _installedPackages = vmInstalledPackages.OrderBy(p => p.Name).ToList();
            _availablePackages = vmAvailablePackages.Values.OrderBy(p => p.Name).ToList<IRPackageViewModel>();
        }
        
        private void DispatchOnMainThreadAsync(Func<Task> callback) {
            _coreShell.DispatchOnMainThreadAsync(callback)
                .Unwrap()
                .SilenceException<OperationCanceledException>()
                .DoNotWait();
        }

        private enum SelectedTab {
            AvailablePackages,
            InstalledPackages,
            LoadedPackages,
        }

        public async Task<int> Search(string searchString, CancellationToken cancellationToken) {
            _searchString = searchString;
            await EnsureInstalledAndAvailablePackagesLoadedAsync();
            switch (_selectedTab) {
                case SelectedTab.AvailablePackages:
                    return Search(_availablePackages, searchString, cancellationToken);
                case SelectedTab.InstalledPackages:
                    return Search(_installedPackages, searchString, cancellationToken);
                case SelectedTab.LoadedPackages:
                    return Search(_loadedPackages, searchString, cancellationToken);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private int Search(IList<IRPackageViewModel> packages, string searchString, CancellationToken cancellationToken) {
            if (string.IsNullOrEmpty(searchString)) {
                _coreShell.DispatchOnUIThread(() => ApplySearch(packages, cancellationToken));
                return packages.Count;
            }

            var filteredPackages = new List<IRPackageViewModel>();
            foreach (var package in packages){
                if (cancellationToken.IsCancellationRequested) {
                    return filteredPackages.Count;
                }

                if (package.Name.StartsWithIgnoreCase(searchString)) {
                    filteredPackages.Add(package);
                }
            }

            _coreShell.DispatchOnUIThread(() => ApplySearch(filteredPackages, cancellationToken));
            return filteredPackages.Count;
        }

        private void ApplySearch(IList<IRPackageViewModel> packages, CancellationToken cancellationToken) {
            _coreShell.AssertIsOnMainThread();
            if (cancellationToken.IsCancellationRequested) {
                return;
            }

            _items.ReplaceWith(packages);
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Wpf;
using Microsoft.Common.Wpf.Collections;
using Microsoft.R.Components.PackageManager.ViewModel;

namespace Microsoft.R.Components.PackageManager.Implementation.ViewModel {
    internal class RPackageManagerViewModel : BindableBase, IRPackageManagerViewModel {
        private readonly IRPackageManager _packageManager;
        private readonly ICoreShell _coreShell;
        private readonly BinaryAsyncLock _availableAndInstalledLock;
        private readonly RangeObservableCollection<object> _items;

        private volatile IList<IRPackageViewModel> _availablePackages;
        private volatile IList<IRPackageViewModel> _installedPackages;

        public RPackageManagerViewModel(IRPackageManager packageManager, ICoreShell coreShell) {
            _packageManager = packageManager;
            _coreShell = coreShell;
            _availableAndInstalledLock = new BinaryAsyncLock();
            _items = new RangeObservableCollection<object>();
            Items = new ReadOnlyObservableCollection<object>(_items);
        }

        public ReadOnlyObservableCollection<object> Items { get; }
        public IRPackageViewModel SelectedPackage { get; }

        public bool IsLoading {
            get { return _isLoading; }
            private set { SetProperty(ref _isLoading, value); }
        }

        private SelectedTab _selectedTab;
        private bool _isLoading;

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
                    LoadLoadedPackages();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
            _coreShell.AssertIsOnMainThread();
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
                    vmPackage.AddDataFromInstalledPackage(installedPackage);
                } else {
                    vmInstalledPackages.Add(RPackageViewModel.CreateInstalled(installedPackage));
                }
            }

            _installedPackages = vmInstalledPackages;
            _availablePackages = vmAvailablePackages.Values.ToList<IRPackageViewModel>();
        }

        private void LoadLoadedPackages() {
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
    }
}

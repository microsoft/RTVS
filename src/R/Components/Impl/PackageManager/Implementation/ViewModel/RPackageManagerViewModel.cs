// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Collections;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.Common.Wpf;
using Microsoft.Common.Wpf.Collections;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Components.PackageManager.Model;
using Microsoft.R.Components.PackageManager.ViewModel;
using Microsoft.R.Components.Settings;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;

namespace Microsoft.R.Components.PackageManager.Implementation.ViewModel {
    internal class RPackageManagerViewModel : BindableBase, IRPackageManagerViewModel {
        private readonly IRPackageManager _packageManager;
        private readonly IRSession _session;
        private readonly IRSettings _settings;
        private readonly ICoreShell _coreShell;
        private readonly BinaryAsyncLock _availableLock;
        private readonly BinaryAsyncLock _installedAndLoadedLock;
        private readonly BatchObservableCollection<object> _items;
        private readonly Queue<string> _errorMessages;

        private volatile IList<IRPackageViewModel> _availablePackages;
        private volatile IList<IRPackageViewModel> _installedPackages;
        private volatile IList<IRPackageViewModel> _loadedPackages;
        private volatile string _searchString;

        private SelectedTab _selectedTab;
        private bool _isLoading;
        private string _firstError;
        private bool _hasMultipleErrors;
        private IRPackageViewModel _selectedPackage;

        public RPackageManagerViewModel(IRPackageManager packageManager, IRSession session, IRSettings settings, ICoreShell coreShell) {
            _packageManager = packageManager;
            _session = session;
            _settings = settings;
            _coreShell = coreShell;
            _selectedTab = SelectedTab.None;
            _availablePackages = new List<IRPackageViewModel>();
            _installedPackages = new List<IRPackageViewModel>();
            _loadedPackages = new List<IRPackageViewModel>();
            _availableLock = new BinaryAsyncLock();
            _installedAndLoadedLock = new BinaryAsyncLock();
            _items = new BatchObservableCollection<object>();
            _errorMessages = new Queue<string>();
            Items = new ReadOnlyObservableCollection<object>(_items);

            _session.Mutated += RSessionMutated;
            _session.PackagesInstalled += OnPackagesInstalled;
            _session.PackagesRemoved += OnPackagesRemoved;
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

        public string FirstError {
            get { return _firstError; }
            private set { SetProperty(ref _firstError, value); }
        }

        public bool HasMultipleErrors {
            get { return _hasMultipleErrors; }
            private set { SetProperty(ref _hasMultipleErrors, value); }
        }

        public bool ShowPackageManagerDisclaimer {
            get { return _settings.ShowPackageManagerDisclaimer; }
            set {
                _settings.ShowPackageManagerDisclaimer = value;
                OnPropertyChanged();
            }
        }

        public async Task ReloadItemsAsync() {
            await _coreShell.SwitchToMainThreadAsync();

            var startingTab = _selectedTab;
            switch (_selectedTab) {
                case SelectedTab.AvailablePackages:
                    await ReloadAvailablePackagesAsync();
                    break;
                case SelectedTab.InstalledPackages:
                    await ReloadInstalledAndLoadedPackagesAsync();
                    break;
                case SelectedTab.LoadedPackages:
                    await ReloadLoadedPackagesAsync();
                    break;
                case SelectedTab.None:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            await ReplaceItemsAsync(startingTab);
        }

        public void SelectPackage(IRPackageViewModel package) {
            _coreShell.AssertIsOnMainThread();
            if (package == _selectedPackage) {
                return;
            }

            SelectedPackage = package;
        }

        public async Task DefaultActionAsync() {
            await _coreShell.SwitchToMainThreadAsync();
            if (SelectedPackage == null) {
                return;
            }

            // Available => Installed => Loaded
            var package = SelectedPackage;
            if (!package.IsInstalled) {
                await InstallAsync(package);
            } else if (!package.IsLoaded) {
                await LoadAsync(package);
            }
        }

        public async Task InstallAsync(IRPackageViewModel package) {
            await _coreShell.SwitchToMainThreadAsync();
            if (package.IsInstalled || package.IsChanging) {
                return;
            }

            BeforeLoadUnload(package);
            var startingTab = _selectedTab;

            try {
                var libPath = await _packageManager.GetLibraryPathAsync();
                await _packageManager.InstallPackageAsync(package.Name, libPath);
            } catch (RHostDisconnectedException) {
                AddErrorMessage(string.Format(CultureInfo.CurrentCulture, Resources.PackageManager_CantInstallPackageNoRSession, package.Name));
            } catch (RPackageManagerException ex) {
                AddErrorMessage(ex.Message);
            }

            await ReloadInstalledAndLoadedPackagesAsync();
            await AfterLoadUnloadAsync(package, startingTab);
        }

        public async Task UpdateAsync(IRPackageViewModel package) {
            await _coreShell.SwitchToMainThreadAsync();
            if (!package.IsInstalled || package.IsChanging) {
                return;
            }

            var confirmUpdate = _coreShell.ShowMessage(string.Format(CultureInfo.CurrentCulture, Resources.PackageManager_PackageUpdateWarning, package.Name), MessageButtons.YesNo);
            if (confirmUpdate != MessageButtons.Yes) {
                return;
            }

            var startingTab = _selectedTab;
            BeforeLoadUnload(package);

            await UpdateImplAsync(package);
            await AfterLoadUnloadAsync(package, startingTab);
        }

        private async Task ReplaceItemsAsync(SelectedTab startingTab) {
            if (startingTab == _selectedTab) {
                switch (_selectedTab) {
                    case SelectedTab.AvailablePackages:
                        await ReplaceItemsAsync(_availablePackages);
                        break;
                    case SelectedTab.InstalledPackages:
                        await ReplaceItemsAsync(_installedPackages);
                        break;
                    case SelectedTab.LoadedPackages:
                        await ReplaceItemsAsync(_loadedPackages);
                        break;
                }
                IsLoading = false;
            }
        }

        private async Task UpdateImplAsync(IRPackageViewModel package) {
            await _coreShell.SwitchToMainThreadAsync();

            if (package.IsLoaded) {
                try {
                    await _packageManager.UnloadPackageAsync(package.Name);
                } catch (RHostDisconnectedException) {
                    AddErrorMessage(string.Format(CultureInfo.CurrentCulture, Resources.PackageManager_CantUnloadPackageNoRSession, package.Name));
                } catch (RPackageManagerException ex) {
                    AddErrorMessage(ex.Message);
                }
                await ReloadLoadedPackagesAsync();
            }

            if (!package.IsLoaded) {
                try {
                    var libPath = package.LibraryPath.ToRPath();
                    try {
                        var packageLockState = await _packageManager.UpdatePackageAsync(package.Name, libPath);
                        if (packageLockState != PackageLockState.Unlocked) {
                            ShowPackageLockedMessage(packageLockState, package.Name);
                        }
                    } catch (RHostDisconnectedException) {
                        AddErrorMessage(string.Format(CultureInfo.CurrentCulture, Resources.PackageManager_CantUpdatePackageNoRSession, package.Name));
                    }
                } catch (RPackageManagerException ex) {
                    AddErrorMessage(ex.Message);
                }
            }

            await ReloadInstalledAndLoadedPackagesAsync();
        }

        public async Task UninstallAsync(IRPackageViewModel package) {
            await _coreShell.SwitchToMainThreadAsync();
            if (!package.IsInstalled || package.IsChanging) {
                return;
            }

            var confirmUninstall = _coreShell.ShowMessage(string.Format(CultureInfo.CurrentCulture, Resources.PackageManager_PackageUninstallWarning, package.Name, package.LibraryPath), MessageButtons.YesNo);
            if (confirmUninstall != MessageButtons.Yes) {
                return;
            }

            BeforeLoadUnload(package);
            var startingTab = _selectedTab;

            if (package.IsLoaded) {
                try {
                    await _packageManager.UnloadPackageAsync(package.Name);
                } catch (RHostDisconnectedException) {
                    AddErrorMessage(string.Format(CultureInfo.CurrentCulture, Resources.PackageManager_CantUnloadPackageNoRSession, package.Name));
                } catch (RPackageManagerException ex) {
                    AddErrorMessage(ex.Message);
                }
                await ReloadLoadedPackagesAsync();
            }

            if (!package.IsLoaded) {
                try {
                    var libPath = package.LibraryPath.ToRPath();
                    var packageLockState = await _packageManager.UninstallPackageAsync(package.Name, libPath);
                    if (packageLockState != PackageLockState.Unlocked) {
                        ShowPackageLockedMessage(packageLockState, package.Name);
                    }
                } catch (RHostDisconnectedException) {
                    AddErrorMessage(string.Format(CultureInfo.CurrentCulture, Resources.PackageManager_CantUninstallPackageNoRSession, package.Name));
                } catch (RPackageManagerException ex) {
                    AddErrorMessage(ex.Message);
                }

                await ReloadInstalledAndLoadedPackagesAsync();
            }

            await AfterLoadUnloadAsync(package, startingTab);
        }

        public async Task LoadAsync(IRPackageViewModel package) {
            await _coreShell.SwitchToMainThreadAsync();
            if (package.IsLoaded) {
                return;
            }

            BeforeLoadUnload(package);
            var startingTab = _selectedTab;

            try {
                await _packageManager.LoadPackageAsync(package.Name, package.LibraryPath.ToRPath());
            } catch (RHostDisconnectedException) {
                AddErrorMessage(string.Format(CultureInfo.CurrentCulture, Resources.PackageManager_CantLoadPackageNoRSession, package.Name));
            } catch (RPackageManagerException ex) {
                AddErrorMessage(ex.Message);
            }

            await ReloadLoadedPackagesAsync();
            await AfterLoadUnloadAsync(package, startingTab);
        }

        public async Task UnloadAsync(IRPackageViewModel package) {
            await _coreShell.SwitchToMainThreadAsync();

            if (!package.IsLoaded) {
                return;
            }

            BeforeLoadUnload(package);
            var startingTab = _selectedTab;

            try {
                await _packageManager.UnloadPackageAsync(package.Name);
            } catch (RHostDisconnectedException) {
                AddErrorMessage(string.Format(CultureInfo.CurrentCulture, Resources.PackageManager_CantUnloadPackageNoRSession, package.Name));
            } catch (RPackageManagerException ex) {
                AddErrorMessage(ex.Message);
            }

            await ReloadLoadedPackagesAsync();
            await AfterLoadUnloadAsync(package, startingTab);
        }

        private void BeforeLoadUnload(IRPackageViewModel package) {
            if (_selectedTab == SelectedTab.InstalledPackages || _selectedTab == SelectedTab.LoadedPackages) {
                IsLoading = true;
            }
            package.IsChanging = true;
        }

        private async Task AfterLoadUnloadAsync(IRPackageViewModel package, SelectedTab startingTab) {
            await ReplaceItemsAsync(startingTab);
            package.IsChanging = false;
        }

        public void DismissErrorMessage() {
            _coreShell.AssertIsOnMainThread();
            if (HasMultipleErrors) {
                FirstError = _errorMessages.Dequeue();
                HasMultipleErrors = _errorMessages.Count > 0;
            } else {
                FirstError = null;
                HasMultipleErrors = false;
            }
        }

        public void DismissAllErrorMessages() {
            _coreShell.AssertIsOnMainThread();
            FirstError = null;
            HasMultipleErrors = false;
            _errorMessages.Clear();
        }

        private void AddErrorMessage(string message) {
            _coreShell.AssertIsOnMainThread();
            if (FirstError == null) {
                FirstError = message;
            } else {
                _errorMessages.Enqueue(message);
                HasMultipleErrors = true;
            }
        }

        private void ShowPackageLockedMessage(PackageLockState packageLockState, string packageName) {
            switch (packageLockState) {
                case PackageLockState.LockedByRSession:
                    _coreShell.ShowErrorMessage(string.Format(CultureInfo.CurrentCulture, Resources.PackageManager_PackageLockedByRSession, packageName));
                    break;
                case PackageLockState.LockedByOther:
                    _coreShell.ShowErrorMessage(string.Format(CultureInfo.CurrentCulture, Resources.PackageManager_PackageLocked, packageName));
                    break;
            }
        }
        
        public async Task SwitchToAvailablePackagesAsync() {
            if (await SetTabAsync(SelectedTab.AvailablePackages)) {
                if (!_availableLock.IsSet) {
                    await EnsureAvailablePackagesLoadedAsync();
                }
                await ReplaceItemsAsync(SelectedTab.AvailablePackages);
            }
        }

        private async Task EnsureAvailablePackagesLoadedAsync() {
            var lockToken = await _availableLock.WaitAsync();
            try {
                if (!lockToken.IsSet) {
                    await LoadAvailablePackagesAsync();
                }
            } catch (RPackageManagerException ex) {
                _coreShell.DispatchOnUIThread(() => AddErrorMessage(ex.Message));
            } finally {
                lockToken.Set();
            }
        }

        private async Task LoadAvailablePackagesAsync() {
            await TaskUtilities.SwitchToBackgroundThread();

            var vmAvailablePackages = new List<IRPackageViewModel>();
            var availablePackages = await _packageManager.GetAvailablePackagesAsync();

            var installedPackages = _installedPackages.ToDictionary(p => p.Name, p => p);
            foreach (var package in availablePackages) {
                IRPackageViewModel installedPackage;
                if (installedPackages.TryGetValue(package.Package, out installedPackage)) {
                    installedPackage.UpdateAvailablePackageDetails(package);
                    vmAvailablePackages.Add(installedPackage);
                } else {
                    vmAvailablePackages.Add(RPackageViewModel.CreateAvailable(package, this));
                }
            }

            _availablePackages = vmAvailablePackages.OrderBy(p => p.Name).ToList();
        }

        public async Task SwitchToInstalledPackagesAsync() {
            if (await SetTabAsync(SelectedTab.InstalledPackages)) {
                await ReloadInstalledAndLoadedPackagesAsync();
                await ReplaceItemsAsync(SelectedTab.InstalledPackages);
            }
        }

        private async Task<bool> SetTabAsync(SelectedTab tab) {
            await _coreShell.SwitchToMainThreadAsync();
            if (_selectedTab != tab) {
                _selectedTab = tab;
                IsLoading = true;
                return true;
            }
            return false;
        }

        private async Task ReloadAvailablePackagesAsync() {
            IsLoading = true;
            await ReloadInstalledAndLoadedPackagesAsync();
            await ReplaceItemsAsync(SelectedTab.AvailablePackages);
        }

        private async Task ReloadInstalledAndLoadedPackagesAsync() {
            var lockToken = await _installedAndLoadedLock.ResetAsync();
            try {
                if (!lockToken.IsSet) {
                    await LoadInstalledAndLoadedPackagesAsync();
                }
            } catch (RPackageManagerException ex) {
                _coreShell.DispatchOnUIThread(() => AddErrorMessage(ex.Message));
            } finally {
                lockToken.Set();
            }
        }

        private async Task LoadInstalledAndLoadedPackagesAsync() {
            await TaskUtilities.SwitchToBackgroundThread();

            var markUninstalledAndUnloadedTask = MarkUninstalledAndUnloaded();
            var getInstalledPackagesTask = _packageManager.GetInstalledPackagesAsync();
            await Task.WhenAll(markUninstalledAndUnloadedTask, getInstalledPackagesTask);

            var installedPackages = getInstalledPackagesTask.Result;
            if (!_availableLock.IsSet) {
                var vmInstalledPackages = installedPackages
                    .Select(package => RPackageViewModel.CreateInstalled(package, this))
                    .OrderBy(p => p.Name)
                    .ToList<IRPackageViewModel>();

                IdentifyRemovablePackages(vmInstalledPackages);

                await UpdateLoadedPackages(vmInstalledPackages);
                _installedPackages = vmInstalledPackages;

                EnsureAvailablePackagesLoadedAsync().DoNotWait();
            } else {
                var vmAvailablePackages = _availablePackages.ToDictionary(k => k.Name);
                var vmInstalledPackages = new List<IRPackageViewModel>();

                foreach (var installedPackage in installedPackages) {
                    IRPackageViewModel vmPackage;
                    if (vmAvailablePackages.TryGetValue(installedPackage.Package, out vmPackage)) {
                        vmPackage.AddDetails(installedPackage, true);
                        vmInstalledPackages.Add(vmPackage);
                    } else {
                        vmInstalledPackages.Add(RPackageViewModel.CreateInstalled(installedPackage, this));
                    }
                }

                IdentifyRemovablePackages(vmInstalledPackages);
                vmInstalledPackages = vmInstalledPackages.OrderBy(p => p.Name).ToList();

                await UpdateLoadedPackages(vmInstalledPackages);
                _installedPackages = vmInstalledPackages;
            }
        }

        private static void IdentifyRemovablePackages(ICollection<IRPackageViewModel> packages) {
            var basePackage = packages.FirstOrDefault(x => x.Name.EqualsOrdinal("base"));
            if (basePackage == null) {
                return;
            }

            foreach (var p in packages) {
                p.CanBeUninstalled = !p.LibraryPath.EqualsIgnoreCase(basePackage.LibraryPath);
            }
        }

        private async Task ReloadLoadedPackagesAsync() {
            await TaskUtilities.SwitchToBackgroundThread();
            try {
                var currentLoadedPackages = _loadedPackages;
                var currentInstalledPackages = _installedPackages;
                List<string> loadedPackageNames;
                try {
                    loadedPackageNames = (await _packageManager.GetLoadedPackagesAsync()).OrderBy(n => n).ToList();
                } catch (RHostDisconnectedException) {
                    _coreShell.DispatchOnUIThread(() => AddErrorMessage(Resources.PackageManager_NoLoadedPackagesNoRSession));
                    loadedPackageNames = new List<string>();
                }

                if (loadedPackageNames.Equals(currentLoadedPackages, (n, p) => n.EqualsIgnoreCase(p.Name))) {
                    return;
                }

                await UpdateLoadedPackages(currentInstalledPackages, loadedPackageNames);
            } catch (RPackageManagerException ex) {
                _coreShell.DispatchOnUIThread(() => AddErrorMessage(ex.Message));
            }
        }

        private async Task UpdateLoadedPackages(IList<IRPackageViewModel> installedPackages, IList<string> loadedPackageNames = null) {
            try {
                loadedPackageNames = loadedPackageNames ?? await _packageManager.GetLoadedPackagesAsync();
            } catch (RHostDisconnectedException) {
                _coreShell.DispatchOnUIThread(() => AddErrorMessage(Resources.PackageManager_NoLoadedPackagesNoRSession));
                loadedPackageNames = new List<string>();
            }

            var vmLoadedPackages = new List<IRPackageViewModel>();
            foreach (var package in installedPackages) {
                package.IsLoaded = loadedPackageNames.Contains(package.Name);
                if (package.IsLoaded) {
                    vmLoadedPackages.Add(package);
                }
            }

            _loadedPackages = vmLoadedPackages;
        }

        private async Task MarkUninstalledAndUnloaded() {
            await _coreShell.SwitchToMainThreadAsync();

            foreach (var package in _installedPackages) {
                package.IsInstalled = false;
                package.IsLoaded = false;
                package.IsChanging = false;
            }
        }

        public async Task SwitchToLoadedPackagesAsync() {
            if (await SetTabAsync(SelectedTab.LoadedPackages)) {
                if (!_installedAndLoadedLock.IsSet) {
                    await ReloadInstalledAndLoadedPackagesAsync();
                }
                await ReplaceItemsAsync(SelectedTab.LoadedPackages);
            }
        }

        private async Task ReplaceItemsAsync(IList<IRPackageViewModel> packages) {
            await _coreShell.SwitchToMainThreadAsync();

            if (string.IsNullOrEmpty(_searchString)) {
                _items.ReplaceWith(packages);
                UpdateSelectedPackage(packages);
            } else {
                Search(packages, _searchString, CancellationToken.None);
            }
        }

        private void UpdateSelectedPackage(IList<IRPackageViewModel> packages) {
            if (packages.Count == 0) {
                SelectedPackage = null;
                return;
            }

            var oldSelectedPackageName = SelectedPackage?.Name;
            SelectPackage(packages[0]);

            var selectedPackage = oldSelectedPackageName != null
                ? packages.FirstOrDefault(p => p.Name.EqualsIgnoreCase(oldSelectedPackageName))
                : null;
            if (selectedPackage != null) {
                SelectPackage(selectedPackage);
            }
        }

        public async Task<int> Search(string searchString, CancellationToken cancellationToken) {
            _searchString = searchString;
            switch (_selectedTab) {
                case SelectedTab.AvailablePackages:
                    await EnsureAvailablePackagesLoadedAsync();
                    return Search(_availablePackages, searchString, cancellationToken);
                case SelectedTab.InstalledPackages:
                    await ReloadInstalledAndLoadedPackagesAsync();
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
            foreach (var package in packages) {
                if (cancellationToken.IsCancellationRequested) {
                    return filteredPackages.Count;
                }

                if (package.Name.ContainsIgnoreCase(searchString)) {
                    filteredPackages.Add(package);
                }
            }

            // Preset results as:
            // 1. Exact match
            // 2. Starts with the search term
            // 3. Everything else
            IList<IRPackageViewModel> result = filteredPackages;
            var exact = filteredPackages.Where(x => x.Name.EqualsOrdinal(searchString)).ToList();
            var startsWith = filteredPackages.Where(x => x.Name.StartsWith(searchString, StringComparison.Ordinal) && !x.Name.EqualsOrdinal(searchString)).ToList();
            if (!cancellationToken.IsCancellationRequested) {
                var remainder = filteredPackages.Except(startsWith).Except(exact);
                if (!cancellationToken.IsCancellationRequested) {
                    result = exact.Concat(startsWith).Concat(remainder).ToList();
                }
            }

            _coreShell.DispatchOnUIThread(() => ApplySearch(result, cancellationToken));
            return result.Count;
        }

        private void ApplySearch(IList<IRPackageViewModel> packages, CancellationToken cancellationToken) {
            _coreShell.AssertIsOnMainThread();
            if (cancellationToken.IsCancellationRequested) {
                return;
            }

            _items.ReplaceWith(packages);
            UpdateSelectedPackage(packages);
        }

        private void RSessionMutated(object sender, EventArgs e) {
            ReloadLoadedPackagesAsync()
                .ContinueWith(t => _coreShell.DispatchOnUIThread(async () => {
                    await ReplaceItemsAsync(SelectedTab.LoadedPackages);
                }))
                .DoNotWait();
        }

        private void OnPackagesInstalled(object sender, EventArgs e) {
            ReloadItemsAsync().DoNotWait();
        }

        private void OnPackagesRemoved(object sender, EventArgs e) {
            ReloadItemsAsync().DoNotWait();
        }

        public void Dispose() {
            _session.Mutated -= RSessionMutated;
            _session.PackagesInstalled -= OnPackagesInstalled;
            _session.PackagesRemoved -= OnPackagesRemoved;
        }

        private enum SelectedTab {
            None,
            AvailablePackages,
            InstalledPackages,
            LoadedPackages,
        }
    }
}

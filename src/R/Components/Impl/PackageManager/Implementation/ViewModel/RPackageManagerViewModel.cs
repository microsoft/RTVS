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
using Microsoft.R.Components.PackageManager.Model;
using Microsoft.R.Components.PackageManager.ViewModel;
using Microsoft.R.Components.Settings;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;

namespace Microsoft.R.Components.PackageManager.Implementation.ViewModel {
    internal class RPackageManagerViewModel : BindableBase, IRPackageManagerViewModel {
        private readonly IRPackageManager _packageManager;
        private readonly IRSettings _settings;
        private readonly ICoreShell _coreShell;
        private readonly BinaryAsyncLock _availableLock;
        private readonly BinaryAsyncLock _installedAndLoadedLock;
        private readonly BatchObservableCollection<object> _items;
        private readonly List<ErrorMessage> _errorMessages;

        private volatile IList<IRPackageViewModel> _availablePackages;
        private volatile IList<IRPackageViewModel> _installedPackages;
        private volatile IList<IRPackageViewModel> _loadedPackages;
        private volatile string _searchString;

        private Tab _selectedTab;
        private bool _isLoading;
        private string _firstError;
        private ErrorMessageType _firstErrorType;
        private bool _hasMultipleErrors;
        private IRPackageViewModel _selectedPackage;

        public RPackageManagerViewModel(IRPackageManager packageManager, IRSettings settings, ICoreShell coreShell) {
            _packageManager = packageManager;
            _settings = settings;
            _coreShell = coreShell;
            _selectedTab = Tab.None;
            _availablePackages = new List<IRPackageViewModel>();
            _installedPackages = new List<IRPackageViewModel>();
            _loadedPackages = new List<IRPackageViewModel>();
            _availableLock = new BinaryAsyncLock();
            _installedAndLoadedLock = new BinaryAsyncLock();
            _items = new BatchObservableCollection<object>();
            _errorMessages = new List<ErrorMessage>();
            Items = new ReadOnlyObservableCollection<object>(_items);

            _packageManager.AvailablePackagesInvalidated += AvailablePackagesInvalidated;
            _packageManager.InstalledPackagesInvalidated += InstalledPackagesInvalidated;
            _packageManager.LoadedPackagesInvalidated += LoadedPackagesInvalidated;
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

        public async Task ReloadCurrentTabAsync() {
            await _coreShell.SwitchToMainThreadAsync();

            var selectedTab = _selectedTab;
            switch (selectedTab) {
                case Tab.AvailablePackages:
                    await ReloadAvailablePackagesAsync();
                    break;
                case Tab.InstalledPackages:
                    await ReloadInstalledAndLoadedPackagesAsync();
                    break;
                case Tab.LoadedPackages:
                    await ReloadLoadedPackagesAsync();
                    break;
                case Tab.None:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            await ReplaceItemsAsync(selectedTab);
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
                AddErrorMessage(Resources.PackageManager_CantInstallPackageNoRSession.FormatCurrent(package.Name), ErrorMessageType.PackageOperations);
            } catch (RPackageManagerException ex) {
                AddErrorMessage(ex.Message, ErrorMessageType.PackageOperations);
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

        private async Task ReplaceItemsAsync(Tab startingTab) {
            if (startingTab == _selectedTab) {
                switch (_selectedTab) {
                    case Tab.AvailablePackages:
                        await ReplaceItemsAsync(_availablePackages);
                        break;
                    case Tab.InstalledPackages:
                        await ReplaceItemsAsync(_installedPackages);
                        break;
                    case Tab.LoadedPackages:
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
                    AddErrorMessage(Resources.PackageManager_CantUnloadPackageNoRSession.FormatCurrent(package.Name), ErrorMessageType.PackageOperations);
                } catch (RPackageManagerException ex) {
                    AddErrorMessage(ex.Message, ErrorMessageType.PackageOperations);
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
                        AddErrorMessage(Resources.PackageManager_CantUpdatePackageNoRSession.FormatCurrent(package.Name), ErrorMessageType.PackageOperations);
                    }
                } catch (RPackageManagerException ex) {
                    AddErrorMessage(ex.Message, ErrorMessageType.PackageOperations);
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
                    AddErrorMessage(Resources.PackageManager_CantUnloadPackageNoRSession.FormatCurrent(package.Name), ErrorMessageType.PackageOperations);
                } catch (RPackageManagerException ex) {
                    AddErrorMessage(ex.Message, ErrorMessageType.PackageOperations);
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
                    AddErrorMessage(Resources.PackageManager_CantUninstallPackageNoRSession.FormatCurrent(package.Name), ErrorMessageType.PackageOperations);
                } catch (RPackageManagerException ex) {
                    AddErrorMessage(ex.Message, ErrorMessageType.PackageOperations);
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
                AddErrorMessage(Resources.PackageManager_CantLoadPackageNoRSession.FormatCurrent(package.Name), ErrorMessageType.PackageOperations);
            } catch (RPackageManagerException ex) {
                AddErrorMessage(ex.Message, ErrorMessageType.PackageOperations);
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
                AddErrorMessage(Resources.PackageManager_CantUnloadPackageNoRSession.FormatCurrent(package.Name), ErrorMessageType.PackageOperations);
            } catch (RPackageManagerException ex) {
                AddErrorMessage(ex.Message, ErrorMessageType.PackageOperations);
            }

            await ReloadLoadedPackagesAsync();
            await AfterLoadUnloadAsync(package, startingTab);
        }

        private void BeforeLoadUnload(IRPackageViewModel package) {
            if (_selectedTab == Tab.InstalledPackages || _selectedTab == Tab.LoadedPackages) {
                IsLoading = true;
            }
            package.IsChanging = true;
        }

        private async Task AfterLoadUnloadAsync(IRPackageViewModel package, Tab startingTab) {
            await ReplaceItemsAsync(startingTab);
            package.IsChanging = false;
        }

        public void DismissErrorMessage() {
            _coreShell.AssertIsOnMainThread();
            if (HasMultipleErrors) {
                HasMultipleErrors = _errorMessages.Count > 0;
                FirstError = _errorMessages[0].Message;
                _firstErrorType = _errorMessages[0].Type;
                _errorMessages.RemoveAt(0);
            } else {
                FirstError = null;
                _firstErrorType = ErrorMessageType.NoError;
                HasMultipleErrors = false;
            }
        }
        
        private void DismissErrorMessages(ErrorMessageType messageType) {
            _coreShell.AssertIsOnMainThread();
            _errorMessages.RemoveWhere(e => e.Type == messageType);
            HasMultipleErrors = _errorMessages.Count > 0;

            if (_firstErrorType == messageType) {
                DismissErrorMessage();
            }
        }

        public void DismissAllErrorMessages() {
            _coreShell.AssertIsOnMainThread();
            FirstError = null;
            _firstErrorType = ErrorMessageType.NoError;
            HasMultipleErrors = false;
            _errorMessages.Clear();
        }

        private void AddErrorMessage(string message, ErrorMessageType messageType) {
            _coreShell.AssertIsOnMainThread();
            if (FirstError == null) {
                FirstError = message;
                _firstErrorType = messageType;
            } else {
                _errorMessages.Add(new ErrorMessage(message, messageType));
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
            if (await SetTabAsync(Tab.AvailablePackages)) {
                await EnsureAvailablePackagesLoadedAsync();
                await ReplaceItemsAsync(Tab.AvailablePackages);
            }
        }

        private async Task EnsureAvailablePackagesLoadedAsync() {
            var lockToken = await _availableLock.WaitAsync();
            try {
                if (!lockToken.IsSet) {
                    await LoadAvailablePackagesAsync();
                    _coreShell.DispatchOnUIThread(() => DismissErrorMessages(ErrorMessageType.Connection));
                    lockToken.Set();
                }
            } catch (RPackageManagerException ex) {
                _coreShell.DispatchOnUIThread(() => AddErrorMessage(ex.Message, ErrorMessageType.Connection));
            } finally {
                lockToken.Reset();
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
            if (await SetTabAsync(Tab.InstalledPackages)) {
                await ReloadInstalledAndLoadedPackagesAsync();
                await ReplaceItemsAsync(Tab.InstalledPackages);
            }
        }

        private async Task<bool> SetTabAsync(Tab tab) {
            await _coreShell.SwitchToMainThreadAsync();
            if (_selectedTab != tab) {
                _selectedTab = tab;
                IsLoading = true;
                return true;
            }
            return false;
        }

        private async Task ReloadCurrentTabAsync(Tab tab) {
            await _coreShell.SwitchToMainThreadAsync();
            if (tab == _selectedTab) {
                await ReloadCurrentTabAsync();
            }
        }

        private async Task ReloadAvailablePackagesAsync() {
            IsLoading = true;
            await ReloadInstalledAndLoadedPackagesAsync();
            await ReplaceItemsAsync(Tab.AvailablePackages);
        }

        private async Task ReloadInstalledAndLoadedPackagesAsync() {
            var lockToken = await _installedAndLoadedLock.ResetAsync();
            try {
                await LoadInstalledAndLoadedPackagesAsync();
                _coreShell.DispatchOnUIThread(() => DismissErrorMessages(ErrorMessageType.Connection));
                lockToken.Set();
            } catch (RPackageManagerException ex) {
                _coreShell.DispatchOnUIThread(() => AddErrorMessage(ex.Message, ErrorMessageType.Connection));
            } finally {
                lockToken.Reset();
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
                    _coreShell.DispatchOnUIThread(() => DismissErrorMessages(ErrorMessageType.NoRSession));
                } catch (RHostDisconnectedException) {
                    _coreShell.DispatchOnUIThread(() => AddErrorMessage(Resources.PackageManager_NoLoadedPackagesNoRSession, ErrorMessageType.NoRSession));
                    loadedPackageNames = new List<string>();
                }

                if (loadedPackageNames.Equals(currentLoadedPackages, (n, p) => n.EqualsIgnoreCase(p.Name))) {
                    return;
                }

                await UpdateLoadedPackages(currentInstalledPackages, loadedPackageNames);
                _coreShell.DispatchOnUIThread(() => DismissErrorMessages(ErrorMessageType.Connection));
            } catch (RPackageManagerException ex) {
                _coreShell.DispatchOnUIThread(() => AddErrorMessage(ex.Message, ErrorMessageType.Connection));
            }
        }

        private async Task UpdateLoadedPackages(IList<IRPackageViewModel> installedPackages, IList<string> loadedPackageNames = null) {
            try {
                loadedPackageNames = loadedPackageNames ?? await _packageManager.GetLoadedPackagesAsync();
                _coreShell.DispatchOnUIThread(() => DismissErrorMessages(ErrorMessageType.NoRSession));
            } catch (RHostDisconnectedException) {
                _coreShell.DispatchOnUIThread(() => AddErrorMessage(Resources.PackageManager_NoLoadedPackagesNoRSession, ErrorMessageType.NoRSession));
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
            if (await SetTabAsync(Tab.LoadedPackages)) {
                if (!_installedAndLoadedLock.IsSet) {
                    await ReloadInstalledAndLoadedPackagesAsync();
                }
                await ReplaceItemsAsync(Tab.LoadedPackages);
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
                case Tab.AvailablePackages:
                    await EnsureAvailablePackagesLoadedAsync();
                    return Search(_availablePackages, searchString, cancellationToken);
                case Tab.InstalledPackages:
                    await ReloadInstalledAndLoadedPackagesAsync();
                    return Search(_installedPackages, searchString, cancellationToken);
                case Tab.LoadedPackages:
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

        private void AvailablePackagesInvalidated(object sender, EventArgs e) {
            _availableLock.EnqueueReset();
            ReloadCurrentTabAsync(Tab.AvailablePackages).DoNotWait();
        }

        private void InstalledPackagesInvalidated(object sender, EventArgs e) {
            _installedAndLoadedLock.EnqueueReset();
            ReloadCurrentTabAsync(Tab.InstalledPackages).DoNotWait();
        }
        
        private void LoadedPackagesInvalidated(object sender, EventArgs e) {
            _installedAndLoadedLock.EnqueueReset();
            ReloadCurrentTabAsync(Tab.LoadedPackages).DoNotWait();
        }

        public void Dispose() {
            _packageManager.AvailablePackagesInvalidated += AvailablePackagesInvalidated;
            _packageManager.InstalledPackagesInvalidated += InstalledPackagesInvalidated;
            _packageManager.LoadedPackagesInvalidated += LoadedPackagesInvalidated;
        }

        private enum Tab {
            None,
            AvailablePackages,
            InstalledPackages,
            LoadedPackages,
        }

        private enum ErrorMessageType {
            NoError,
            Connection,
            PackageOperations,
            NoRSession,
        }

        private struct ErrorMessage {
            public string Message { get; }
            public ErrorMessageType Type { get; }

            public ErrorMessage(string message, ErrorMessageType type) {
                Message = message;
                Type = type;
            }
        }
    }
}

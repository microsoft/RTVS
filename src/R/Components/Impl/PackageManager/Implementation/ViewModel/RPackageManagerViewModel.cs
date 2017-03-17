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
using Microsoft.Common.Core.UI;
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
        private readonly ErrorMessageCollection _errorMessages;

        private volatile IList<IRPackageViewModel> _availablePackages;
        private volatile IList<IRPackageViewModel> _installedPackages;
        private volatile IList<IRPackageViewModel> _loadedPackages;
        private volatile string _searchString;

        private Tab _selectedTab;
        private bool _isLoading;
        private string _firstError;
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
            _errorMessages = new ErrorMessageCollection(this);
            Items = new ReadOnlyObservableCollection<object>(_items);

            _packageManager.AvailablePackagesInvalidated += AvailablePackagesInvalidated;
            _packageManager.InstalledPackagesInvalidated += InstalledPackagesInvalidated;
            _packageManager.LoadedPackagesInvalidated += LoadedPackagesInvalidated;
        }

        public ReadOnlyObservableCollection<object> Items { get; }

        public bool IsRemoteSession => _packageManager.IsRemoteSession;

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

        public async Task ReloadCurrentTabAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            await _coreShell.SwitchToMainThreadAsync(cancellationToken);
            await ReloadTabContentAsync(_selectedTab, cancellationToken);
        }

        public void SelectPackage(IRPackageViewModel package) {
            _coreShell.AssertIsOnMainThread();
            if (package == _selectedPackage) {
                return;
            }

            SelectedPackage = package;
        }

        public async Task DefaultActionAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            await _coreShell.SwitchToMainThreadAsync(cancellationToken);
            if (SelectedPackage == null) {
                return;
            }

            // Available => Installed => Loaded
            var package = SelectedPackage;
            if (!package.IsInstalled) {
                await InstallAsync(package, cancellationToken);
            } else if (!package.IsLoaded) {
                await LoadAsync(package, cancellationToken);
            }
        }

        public async Task InstallAsync(IRPackageViewModel package, CancellationToken cancellationToken = default(CancellationToken)) {
            await _coreShell.SwitchToMainThreadAsync(cancellationToken);
            if (package.IsInstalled || package.IsChanging) {
                return;
            }

            BeforeLoadUnload(package);
            var startingTab = _selectedTab;

            try {
                var libPath = await _packageManager.GetLibraryPathAsync(cancellationToken);
                await _packageManager.InstallPackageAsync(package.Name, libPath, cancellationToken);
            } catch (RHostDisconnectedException) {
                _errorMessages.Add(Resources.PackageManager_CantInstallPackageNoRSession.FormatCurrent(package.Name), ErrorMessageType.PackageOperations);
            } catch (RPackageManagerException ex) {
                _errorMessages.Add(ex.Message, ErrorMessageType.PackageOperations);
            }

            await EnsureInstalledAndLoadedPackagesAsync(true, cancellationToken);
            AfterLoadUnload(package, startingTab);
        }

        public async Task UpdateAsync(IRPackageViewModel package, CancellationToken cancellationToken = default(CancellationToken)) {
            await _coreShell.SwitchToMainThreadAsync(cancellationToken);
            if (!package.IsInstalled || package.IsChanging) {
                return;
            }

            var message = string.Format(CultureInfo.CurrentCulture, Resources.PackageManager_PackageUpdateWarning, package.Name);
            var confirmUpdate = _coreShell.ShowMessage(message, MessageButtons.YesNo);
            if (confirmUpdate != MessageButtons.Yes) {
                return;
            }

            var startingTab = _selectedTab;
            BeforeLoadUnload(package);

            await UpdateImplAsync(package, cancellationToken);
            AfterLoadUnload(package, startingTab);
        }

        private void ReplaceItems(Tab startingTab) {
            _coreShell.AssertIsOnMainThread();

            if (startingTab == _selectedTab) {
                switch (_selectedTab) {
                    case Tab.AvailablePackages:
                        ReplaceItems(_availablePackages);
                        break;
                    case Tab.InstalledPackages:
                        ReplaceItems(_installedPackages);
                        break;
                    case Tab.LoadedPackages:
                        ReplaceItems(_loadedPackages);
                        break;
                }
                IsLoading = false;
            }
        }

        private async Task UpdateImplAsync(IRPackageViewModel package, CancellationToken cancellationToken) {
            await _coreShell.SwitchToMainThreadAsync(cancellationToken);

            if (package.IsLoaded) {
                try {
                    await _packageManager.UnloadPackageAsync(package.Name, cancellationToken);
                } catch (RHostDisconnectedException) {
                    _errorMessages.Add(Resources.PackageManager_CantUnloadPackageNoRSession.FormatCurrent(package.Name), ErrorMessageType.PackageOperations);
                } catch (RPackageManagerException ex) {
                    _errorMessages.Add(ex.Message, ErrorMessageType.PackageOperations);
                }
                await ReloadLoadedPackagesAsync(cancellationToken);
            }

            if (!package.IsLoaded) {
                try {
                    var libPath = package.LibraryPath.ToRPath();
                    try {
                        var packageLockState = await _packageManager.UpdatePackageAsync(package.Name, libPath, cancellationToken);
                        if (packageLockState != PackageLockState.Unlocked) {
                            ShowPackageLockedMessage(packageLockState, package.Name);
                        }
                    } catch (RHostDisconnectedException) {
                        _errorMessages.Add(Resources.PackageManager_CantUpdatePackageNoRSession.FormatCurrent(package.Name), ErrorMessageType.PackageOperations);
                    }
                } catch (RPackageManagerException ex) {
                    _errorMessages.Add(ex.Message, ErrorMessageType.PackageOperations);
                }
            }

            await EnsureInstalledAndLoadedPackagesAsync(true, cancellationToken);
        }

        public async Task UninstallAsync(IRPackageViewModel package, CancellationToken cancellationToken = default(CancellationToken)) {
            await _coreShell.SwitchToMainThreadAsync(cancellationToken);
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
                    await _packageManager.UnloadPackageAsync(package.Name, cancellationToken);
                } catch (RHostDisconnectedException) {
                    _errorMessages.Add(Resources.PackageManager_CantUnloadPackageNoRSession.FormatCurrent(package.Name), ErrorMessageType.PackageOperations);
                } catch (RPackageManagerException ex) {
                    _errorMessages.Add(ex.Message, ErrorMessageType.PackageOperations);
                }
                await ReloadLoadedPackagesAsync(cancellationToken);
            }

            if (!package.IsLoaded) {
                try {
                    var libPath = package.LibraryPath.ToRPath();
                    var packageLockState = await _packageManager.UninstallPackageAsync(package.Name, libPath, cancellationToken);
                    if (packageLockState != PackageLockState.Unlocked) {
                        ShowPackageLockedMessage(packageLockState, package.Name);
                    }
                } catch (RHostDisconnectedException) {
                    _errorMessages.Add(Resources.PackageManager_CantUninstallPackageNoRSession.FormatCurrent(package.Name), ErrorMessageType.PackageOperations);
                } catch (RPackageManagerException ex) {
                    _errorMessages.Add(ex.Message, ErrorMessageType.PackageOperations);
                }

                await EnsureInstalledAndLoadedPackagesAsync(true, cancellationToken);
            }

            AfterLoadUnload(package, startingTab);
        }

        public async Task LoadAsync(IRPackageViewModel package, CancellationToken cancellationToken = default(CancellationToken)) {
            await _coreShell.SwitchToMainThreadAsync(cancellationToken);
            if (package.IsLoaded) {
                return;
            }

            BeforeLoadUnload(package);
            var startingTab = _selectedTab;

            try {
                await _packageManager.LoadPackageAsync(package.Name, package.LibraryPath.ToRPath(), cancellationToken);
            } catch (RHostDisconnectedException) {
                _errorMessages.Add(Resources.PackageManager_CantLoadPackageNoRSession.FormatCurrent(package.Name), ErrorMessageType.PackageOperations);
            } catch (RPackageManagerException ex) {
                _errorMessages.Add(ex.Message, ErrorMessageType.PackageOperations);
            }

            await ReloadLoadedPackagesAsync(cancellationToken);
            AfterLoadUnload(package, startingTab);
        }

        public async Task UnloadAsync(IRPackageViewModel package, CancellationToken cancellationToken = default(CancellationToken)) {
            await _coreShell.SwitchToMainThreadAsync(cancellationToken);

            if (!package.IsLoaded) {
                return;
            }

            BeforeLoadUnload(package);
            var startingTab = _selectedTab;

            try {
                await _packageManager.UnloadPackageAsync(package.Name, cancellationToken);
            } catch (RHostDisconnectedException) {
                _errorMessages.Add(Resources.PackageManager_CantUnloadPackageNoRSession.FormatCurrent(package.Name), ErrorMessageType.PackageOperations);
            } catch (RPackageManagerException ex) {
                _errorMessages.Add(ex.Message, ErrorMessageType.PackageOperations);
            }

            await ReloadLoadedPackagesAsync(cancellationToken);
            AfterLoadUnload(package, startingTab);
        }

        private void BeforeLoadUnload(IRPackageViewModel package) {
            if (_selectedTab == Tab.InstalledPackages || _selectedTab == Tab.LoadedPackages) {
                IsLoading = true;
            }
            package.IsChanging = true;
        }

        private void AfterLoadUnload(IRPackageViewModel package, Tab startingTab) {
            _coreShell.AssertIsOnMainThread();
            ReplaceItems(startingTab);
            package.IsChanging = false;
        }

        public void DismissErrorMessage() {
            _coreShell.AssertIsOnMainThread();
            _errorMessages.RemoveCurrent();
        }
        
        public void DismissAllErrorMessages() {
            _coreShell.AssertIsOnMainThread();
            _errorMessages.Clear();
        }
        
        private void ShowPackageLockedMessage(PackageLockState packageLockState, string packageName) {
            switch (packageLockState) {
                case PackageLockState.LockedByRSession:
                    _coreShell.ShowErrorMessage(Resources.PackageManager_PackageLockedByRSession.FormatCurrent(packageName));
                    break;
                case PackageLockState.LockedByOther:
                    _coreShell.ShowErrorMessage(Resources.PackageManager_PackageLocked.FormatCurrent(packageName));
                    break;
            }
        }
        
        public async Task SwitchToAvailablePackagesAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            await _coreShell.SwitchToMainThreadAsync();
            if (SetTab(Tab.AvailablePackages)) {
                await EnsureAvailablePackagesLoadedAsync(false, cancellationToken);
                ReplaceItems(Tab.AvailablePackages);
            }
        }

        private async Task EnsureAvailablePackagesLoadedAsync(bool reload, CancellationToken cancellationToken) {
            var lockToken = reload ? await _availableLock.ResetAsync(cancellationToken) : await _availableLock.WaitAsync(cancellationToken);
            try {
                if (!lockToken.IsSet) {
                    await LoadAvailablePackagesAsync(cancellationToken);
                    _errorMessages.Remove(ErrorMessageType.NoAvailable);
                    lockToken.Set();
                }
            } catch (RPackageManagerException) {
                _errorMessages.Add(Resources.PackageManager_CantLoadAvailablePackages_NoConnection, ErrorMessageType.NoAvailable);
            } finally {
                lockToken.Reset();
            }
        }

        private async Task LoadAvailablePackagesAsync(CancellationToken cancellationToken) {
            await TaskUtilities.SwitchToBackgroundThread();

            var vmAvailablePackages = new List<IRPackageViewModel>();
            var availablePackages = await _packageManager.GetAvailablePackagesAsync(cancellationToken);

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

        public async Task SwitchToInstalledPackagesAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            await _coreShell.SwitchToMainThreadAsync();
            if (SetTab(Tab.InstalledPackages)) {
                await EnsureInstalledAndLoadedPackagesAsync(true, cancellationToken);
                ReplaceItems(Tab.InstalledPackages);
            }
        }

        private bool SetTab(Tab tab) {
            _coreShell.AssertIsOnMainThread();
            if (_selectedTab == tab) {
                return false;
            }

            _selectedTab = tab;
            IsLoading = true;
            return true;
        }

        private async Task ReloadTabContentAsync(Tab tab, CancellationToken cancellationToken = default(CancellationToken)) {
            await _coreShell.SwitchToMainThreadAsync(cancellationToken);
            if (tab == _selectedTab) {
                IsLoading = true;
            }

            switch (tab) {
                case Tab.AvailablePackages:
                    await EnsureAvailablePackagesLoadedAsync(true, cancellationToken);
                    break;
                case Tab.InstalledPackages:
                    await EnsureInstalledAndLoadedPackagesAsync(true, cancellationToken);
                    break;
                case Tab.LoadedPackages:
                    await ReloadLoadedPackagesAsync(cancellationToken);
                    break;
                case Tab.None:
                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ReplaceItems(tab);
        }

        private async Task EnsureInstalledAndLoadedPackagesAsync(bool reload, CancellationToken cancellationToken) {
            var lockToken = reload 
                ? await _installedAndLoadedLock.ResetAsync(cancellationToken)
                : await _installedAndLoadedLock.WaitAsync(cancellationToken);

            if (!lockToken.IsSet) {
                try {
                    await LoadInstalledAndLoadedPackagesAsync(reload, cancellationToken);
                    _errorMessages.Remove(ErrorMessageType.NoInstalled);
                    lockToken.Set();
                } catch (RPackageManagerException) {
                    _errorMessages.Add(Resources.PackageManager_CantLoadInstalledPackages_NoConnection, ErrorMessageType.NoInstalled);
                } finally {
                    lockToken.Reset();
                }
            }
        }

        private async Task LoadInstalledAndLoadedPackagesAsync(bool reload, CancellationToken cancellationToken) {
            await TaskUtilities.SwitchToBackgroundThread();

            var markUninstalledAndUnloadedTask = MarkUninstalledAndUnloaded(cancellationToken);
            var getInstalledPackagesTask = _packageManager.GetInstalledPackagesAsync(cancellationToken);
            await Task.WhenAll(markUninstalledAndUnloadedTask, getInstalledPackagesTask);

            if (reload) {
                _installedPackages = new List<IRPackageViewModel>();
            }
            var installedPackages = getInstalledPackagesTask.Result;

            if (!_availableLock.IsSet) {
                var vmInstalledPackages = installedPackages
                    .Select(package => RPackageViewModel.CreateInstalled(package, this))
                    .OrderBy(p => p.Name)
                    .ToList<IRPackageViewModel>();

                IdentifyRemovablePackages(vmInstalledPackages);

                await UpdateLoadedPackages(vmInstalledPackages, null, cancellationToken);
                _installedPackages = vmInstalledPackages;

                EnsureAvailablePackagesLoadedAsync(false, cancellationToken).DoNotWait();
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

                await UpdateLoadedPackages(vmInstalledPackages, null, cancellationToken);
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

        private async Task ReloadLoadedPackagesAsync(CancellationToken cancellationToken) {
            await TaskUtilities.SwitchToBackgroundThread();
            try {
                var currentLoadedPackages = _loadedPackages;
                var currentInstalledPackages = _installedPackages;
                List<string> loadedPackageNames;
                try {
                    loadedPackageNames = (await _packageManager.GetLoadedPackagesAsync(cancellationToken)).OrderBy(n => n).ToList();
                    _errorMessages.Remove(ErrorMessageType.NoRSession);
                } catch (RHostDisconnectedException) {
                    _errorMessages.Add(Resources.PackageManager_NoLoadedPackagesNoRSession, ErrorMessageType.NoRSession);
                    loadedPackageNames = new List<string>();
                }

                if (loadedPackageNames.Equals(currentLoadedPackages, (n, p) => n.EqualsIgnoreCase(p.Name))) {
                    return;
                }

                await UpdateLoadedPackages(currentInstalledPackages, loadedPackageNames, cancellationToken);
                _errorMessages.Remove(ErrorMessageType.NoLoaded);
            } catch (RPackageManagerException) {
                _errorMessages.Add(Resources.PackageManager_CantLoadLoadedPackages_NoConnection, ErrorMessageType.NoLoaded);
            }
        }

        private async Task UpdateLoadedPackages(IList<IRPackageViewModel> installedPackages, IList<string> loadedPackageNames, CancellationToken cancellationToken) {
            try {
                loadedPackageNames = loadedPackageNames ?? await _packageManager.GetLoadedPackagesAsync(cancellationToken);
                _errorMessages.Remove(ErrorMessageType.NoRSession);
            } catch (RHostDisconnectedException) {
                _errorMessages.Add(Resources.PackageManager_NoLoadedPackagesNoRSession, ErrorMessageType.NoRSession);
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

        private async Task MarkUninstalledAndUnloaded(CancellationToken cancellationToken) {
            await _coreShell.SwitchToMainThreadAsync(cancellationToken);

            foreach (var package in _installedPackages) {
                package.IsInstalled = false;
                package.IsLoaded = false;
                package.IsChanging = false;
            }
        }

        public async Task SwitchToLoadedPackagesAsync(CancellationToken cancellationToken = default(CancellationToken)) {
            await _coreShell.SwitchToMainThreadAsync();
            if (SetTab(Tab.LoadedPackages)) {
                await EnsureInstalledAndLoadedPackagesAsync(false, cancellationToken);
                ReplaceItems(Tab.LoadedPackages);
            }
        }

        private void ReplaceItems(IList<IRPackageViewModel> packages) {
            _coreShell.AssertIsOnMainThread();
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
                    await EnsureAvailablePackagesLoadedAsync(false, cancellationToken);
                    return Search(_availablePackages, searchString, cancellationToken);
                case Tab.InstalledPackages:
                    await EnsureInstalledAndLoadedPackagesAsync(true, cancellationToken);
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
            ReloadTabContentAsync(Tab.AvailablePackages).DoNotWait();
        }

        private void InstalledPackagesInvalidated(object sender, EventArgs e) {
            _installedAndLoadedLock.EnqueueReset();
            ReloadTabContentAsync(Tab.InstalledPackages).DoNotWait();
        }
        
        private void LoadedPackagesInvalidated(object sender, EventArgs e) {
            _installedAndLoadedLock.EnqueueReset();
            ReloadTabContentAsync(Tab.LoadedPackages).DoNotWait();
        }

        public void Dispose() {
            _packageManager.AvailablePackagesInvalidated -= AvailablePackagesInvalidated;
            _packageManager.InstalledPackagesInvalidated -= InstalledPackagesInvalidated;
            _packageManager.LoadedPackagesInvalidated -= LoadedPackagesInvalidated;
        }

        private enum Tab {
            None,
            AvailablePackages,
            InstalledPackages,
            LoadedPackages,
        }

        private enum ErrorMessageType {
            NoAvailable,
            NoInstalled,
            NoLoaded,
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

        private class ErrorMessageCollection {
            private readonly RPackageManagerViewModel _viewModel;
            private readonly List<ErrorMessage> _errorMessages;

            public ErrorMessageCollection(RPackageManagerViewModel viewModel) {
                _viewModel = viewModel;
                _errorMessages = new List<ErrorMessage>();
            }

            public void Add(string message, ErrorMessageType type) {
                message = message.Replace(Environment.NewLine, " ");
                lock (_errorMessages) {
                    _errorMessages.Add(new ErrorMessage(message, type));
                    if (_errorMessages.Count == 1) {
                        _viewModel.FirstError = message;
                    }
                    _viewModel.HasMultipleErrors = _errorMessages.Count > 1;
                }
            }

            public void RemoveCurrent() {
                lock (_errorMessages) {
                    if (_errorMessages.Count > 0) {
                        _errorMessages.RemoveAt(0);
                        _viewModel.FirstError = _errorMessages.Count > 0 ? _errorMessages[0].Message : null;
                    }

                    _viewModel.HasMultipleErrors = _errorMessages.Count > 1;
                }
            }

            public void Remove(ErrorMessageType type) {
                lock (_errorMessages) {
                    _errorMessages.RemoveWhere(e => e.Type == type);
                    _viewModel.FirstError = _errorMessages.Count > 0 ? _errorMessages[0].Message : null;
                    _viewModel.HasMultipleErrors = _errorMessages.Count > 1;
                }
            }

            public void Clear() {
                lock (_errorMessages) {
                    _errorMessages.Clear();
                    _viewModel.FirstError = null;
                    _viewModel.HasMultipleErrors = false;
                }
            }
        }
    }
}

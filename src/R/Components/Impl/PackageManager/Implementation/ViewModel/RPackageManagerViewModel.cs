// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
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

namespace Microsoft.R.Components.PackageManager.Implementation.ViewModel {
    internal class RPackageManagerViewModel : BindableBase, IRPackageManagerViewModel {
        private readonly IRPackageManager _packageManager;
        private readonly IRSession _session;
        private readonly IRSettings _settings;
        private readonly ICoreShell _coreShell;
        private readonly BinaryAsyncLock _availableLock;
        private readonly BinaryAsyncLock _installedAndLoadedLock;
        private readonly BatchObservableCollection<object> _items;

        private volatile IList<IRPackageViewModel> _availablePackages;
        private volatile IList<IRPackageViewModel> _installedPackages;
        private volatile IList<IRPackageViewModel> _loadedPackages;
        private volatile string _searchString;

        private SelectedTab _selectedTab;
        private bool _isLoading;
        private IRPackageViewModel _selectedPackage;
        private static readonly Comparer<IRPackageViewModel> _comparer = Comparer<IRPackageViewModel>.Create((p1, p2) => string.Compare(p1.Name, p2.Name, StringComparison.InvariantCultureIgnoreCase));

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
            Items = new ReadOnlyObservableCollection<object>(_items);

            _session.Mutated += RSessionMutated;
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

        public bool ShowPackageManagerDisclaimer {
            get { return _settings.ShowPackageManagerDisclaimer; }
            set {
                _settings.ShowPackageManagerDisclaimer = value;
                OnPropertyChanged();
            }
        }

        public void SwitchToAvailablePackages() {
            _coreShell.AssertIsOnMainThread();
            if (_selectedTab == SelectedTab.AvailablePackages) {
                return;
            }

            _selectedTab = SelectedTab.AvailablePackages;
            DispatchOnMainThread(SwitchToAvailablePackagesAsync);
        }

        public void SwitchToInstalledPackages() {
            _coreShell.AssertIsOnMainThread();
            if (_selectedTab == SelectedTab.InstalledPackages) {
                return;
            }

            _selectedTab = SelectedTab.InstalledPackages;
            DispatchOnMainThread(SwitchToInstalledPackagesAsync);
        }

        public void SwitchToLoadedPackages() {
            _coreShell.AssertIsOnMainThread();
            if (_selectedTab == SelectedTab.LoadedPackages) {
                return;
            }

            _selectedTab = SelectedTab.LoadedPackages;
            DispatchOnMainThread(SwitchToLoadedPackagesAsync);
        }

        public void ReloadItems() {
            _coreShell.AssertIsOnMainThread();
            switch (_selectedTab) {
                case SelectedTab.AvailablePackages:
                    //LoadAvailablePackages();
                    break;
                case SelectedTab.InstalledPackages:
                    //LoadInstalledPackages();
                    break;
                case SelectedTab.LoadedPackages:
                    break;
                case SelectedTab.None:
                    return;
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
                DispatchOnMainThread(() => AddPackageDetailsAsync(package));
            }
        }

        public void Install(IRPackageViewModel package) {
            if (package.IsInstalled) {
                return;
            }

            DispatchOnMainThread(() => InstallAsync(package));
        }

        public void Update(IRPackageViewModel package) {
            if (package.IsInstalled) {
                return;
            }

            package.IsChanging = true;
            DispatchOnMainThread(() => InstallAsync(package));
        }

        private async Task InstallAsync(IRPackageViewModel package) {
            _coreShell.AssertIsOnMainThread();
            if (_selectedTab == SelectedTab.InstalledPackages) {
                IsLoading = true;
            }

            var libPath = await GetLibPath();
            await _packageManager.InstallPackage(package.Name, libPath);
            await ReloadInstalledAndLoadedPackagesAsync();

            if (_selectedTab == SelectedTab.InstalledPackages) {
                IsLoading = false;
                ReplaceItems(_installedPackages);
            }
        }

        public void Uninstall(IRPackageViewModel package) {
            if (!package.IsInstalled) {
                return;
            }

            package.IsChanging = true;
            DispatchOnMainThread(() => UninstallAsync(package));
        }

        private async Task UninstallAsync(IRPackageViewModel package) {
            _coreShell.AssertIsOnMainThread();
            if (_selectedTab == SelectedTab.InstalledPackages) {
                IsLoading = true;
            }

            var libPath = await GetLibPath();
            await _packageManager.UninstallPackage(package.Name, libPath);
            await ReloadInstalledAndLoadedPackagesAsync();

            if (_selectedTab == SelectedTab.InstalledPackages) {
                IsLoading = false;
                ReplaceItems(_installedPackages);
            }
        }

        public void Load(IRPackageViewModel package) {
            if (package.IsLoaded) {
                return;
            }

            package.IsChanging = true;
            DispatchOnMainThread(() => LoadAsync(package));
        }

        private async Task LoadAsync(IRPackageViewModel package) {
            _coreShell.AssertIsOnMainThread();
            BeforeLoadUnload();

            await _packageManager.LoadPackage(package.Name, package.RepositoryUri?.AbsolutePath.ToRPath());
            await ReloadLoadedPackagesAsync();

            AfterLoadUnload(package);
        }

        public void Unload(IRPackageViewModel package) {
            if (!package.IsLoaded) {
                return;
            }

            package.IsChanging = true;
            DispatchOnMainThread(() => UnloadAsync(package));
        }

        private async Task UnloadAsync(IRPackageViewModel package) {
            _coreShell.AssertIsOnMainThread();
            BeforeLoadUnload();

            await _packageManager.UnloadPackage(package.Name);
            await ReloadLoadedPackagesAsync();

            AfterLoadUnload(package);
        }

        private void BeforeLoadUnload() {
            if (_selectedTab == SelectedTab.InstalledPackages || _selectedTab == SelectedTab.LoadedPackages) {
                IsLoading = true;
            }
        }

        private void AfterLoadUnload(IRPackageViewModel package) {
            package.IsChanging = false;
            if (_selectedTab == SelectedTab.InstalledPackages) {
                IsLoading = false;
                ReplaceItems(_installedPackages);
            }
        }

        private async Task AddPackageDetailsAsync(IRPackageViewModel package) {
            _coreShell.AssertIsOnMainThread();
            var details = await GetAdditionalPackageInfoAsync(package);
            package.AddDetails(details, false);
        }

        private async Task<RPackage> GetAdditionalPackageInfoAsync(IRPackageViewModel package) {
            await TaskUtilities.SwitchToBackgroundThread();
            return await _packageManager.GetAdditionalPackageInfoAsync(package.Name, package.RepositoryText ?? package.RepositoryUri.AbsoluteUri);
        }

        private async Task<string> GetLibPath() {
            var rBasePath = _settings.RBasePath.ToRPath();
            var libPaths = await _packageManager.GetLibraryPathsAsync();
            return libPaths.Select(p => p.ToRPath()).FirstOrDefault(s => !s.StartsWithIgnoreCase(rBasePath));
        }

        private async Task SwitchToAvailablePackagesAsync() {
            _coreShell.AssertIsOnMainThread();
            if (_selectedTab == SelectedTab.AvailablePackages) {
                IsLoading = !_availableLock.IsCompleted;
            }
            
            await EnsureAvailablePackagesLoadedAsync();
            if (_selectedTab == SelectedTab.AvailablePackages) {
                IsLoading = false;
            }

            if (_availablePackages == null) {
                return;
            }
            ReplaceItems(_availablePackages);
        }

        private async Task EnsureAvailablePackagesLoadedAsync() {
            var availablePackagesLoaded = await _availableLock.WaitAsync();
            if (!availablePackagesLoaded) {
                try {
                    await LoadAvailablePackagesAsync();
                } catch (RPackageManagerException ex) { 
                } finally {
                    _availableLock.Release();
                }
            }
        }

        private async Task LoadAvailablePackagesAsync() {
            await TaskUtilities.SwitchToBackgroundThread();
            var availablePackages = await _packageManager.GetAvailablePackagesAsync();
            var vmAvailablePackages = new List<IRPackageViewModel>();

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

        private async Task SwitchToInstalledPackagesAsync() {
            _coreShell.AssertIsOnMainThread();
            if (_selectedTab == SelectedTab.InstalledPackages) {
                IsLoading = true;
            }

            await ReloadInstalledAndLoadedPackagesAsync();
            if (_selectedTab == SelectedTab.InstalledPackages) {
                IsLoading = false;
            }
            
            ReplaceItems(_installedPackages);
        }

        private async Task ReloadInstalledAndLoadedPackagesAsync() {
            _installedAndLoadedLock.ResetIfNotWaiting();
            var areLoaded = await _installedAndLoadedLock.WaitAsync();
            if (areLoaded) {
                return;
            }

            try {
                await LoadInstalledAndLoadedPackagesAsync();
            } catch (RPackageManagerException ex) {
            } finally {
                _installedAndLoadedLock.Release();
            }
        }

        private async Task LoadInstalledAndLoadedPackagesAsync() {
            await TaskUtilities.SwitchToBackgroundThread();

            var markUninstalledAndUnloadedTask =  _coreShell.DispatchOnMainThreadAsync(MarkUninstalledAndUnloaded);
            var getInstalledPackagesTask = _packageManager.GetInstalledPackagesAsync();
            await Task.WhenAll(markUninstalledAndUnloadedTask, getInstalledPackagesTask);

            var installedPackages = getInstalledPackagesTask.Result;
            if (!_availableLock.IsCompleted) {
                var vmInstalledPackages = installedPackages
                    .Select(package => RPackageViewModel.CreateInstalled(package, this))
                    .OrderBy(p => p.Name)
                    .ToList<IRPackageViewModel>();

                await UpdateLoadedPackages(vmInstalledPackages);
                _installedPackages = vmInstalledPackages;
                DispatchOnMainThread(EnsureAvailablePackagesLoadedAsync);
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

                vmInstalledPackages = vmInstalledPackages.OrderBy(p => p.Name).ToList();

                await UpdateLoadedPackages(vmInstalledPackages);
                _installedPackages = vmInstalledPackages;
            }
        }

        private async Task ReloadLoadedPackagesAsync() {
            await TaskUtilities.SwitchToBackgroundThread();
            try {
                var currentLoadedPackages = _loadedPackages;
                var currentInstalledPackages = _installedPackages;
                var loadedPackageNames = (await _packageManager.GetLoadedPackagesAsync()).OrderBy(n => n).ToList();

                if (loadedPackageNames.Equals(currentLoadedPackages, (n, p) => n.EqualsIgnoreCase(p.Name))) {
                    return;
                }

                await UpdateLoadedPackages(currentInstalledPackages, loadedPackageNames);
                _coreShell.DispatchOnUIThread(() => {
                    if (_selectedTab == SelectedTab.LoadedPackages) {
                        IsLoading = false;
                        ReplaceItems(_loadedPackages);
                    }
                });
            } catch (RPackageManagerException ex) {
            } 
        }

        private async Task UpdateLoadedPackages(IList<IRPackageViewModel> installedPackages, IList<string> loadedPackageNames = null) {
            loadedPackageNames = loadedPackageNames ?? await _packageManager.GetLoadedPackagesAsync();

            var vmLoadedPackages = new List<IRPackageViewModel>();
            foreach (var package in installedPackages) {
                package.IsLoaded = loadedPackageNames.Contains(package.Name);
                if (package.IsLoaded) {
                    vmLoadedPackages.Add(package);
                }
            }

            _loadedPackages = vmLoadedPackages;
        }

        private void MarkUninstalledAndUnloaded() {
            _coreShell.AssertIsOnMainThread();
            foreach (var package in _installedPackages) {
                package.IsInstalled = false;
                package.IsLoaded = false;
                package.IsChanging = false;
            }
        }

        private async Task SwitchToLoadedPackagesAsync() {
            _coreShell.AssertIsOnMainThread();
            if (_installedAndLoadedLock.IsCompleted) {
                ReplaceItems(_loadedPackages);
                return;
            }

            if (_selectedTab == SelectedTab.LoadedPackages) {
                IsLoading = true;
            }

            await ReloadInstalledAndLoadedPackagesAsync();
            if (_selectedTab == SelectedTab.LoadedPackages) {
                IsLoading = false;
            }

            ReplaceItems(_loadedPackages);
        }

        private void ReplaceItems(IList<IRPackageViewModel> packages) {
            _coreShell.AssertIsOnMainThread();
            if (string.IsNullOrEmpty(_searchString)) {
                _items.ReplaceWith(packages);
            } else { 
                Search(packages, _searchString, CancellationToken.None);
            }
        }

        private void DispatchOnMainThread(Func<Task> callback) {
            _coreShell.DispatchOnMainThreadAsync(callback)
                .Unwrap()
                .SilenceException<OperationCanceledException>()
                .DoNotWait();
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
            foreach (var package in packages){
                if (cancellationToken.IsCancellationRequested) {
                    return filteredPackages.Count;
                }

                if (package.Name.ContainsIgnoreCase(searchString)) {
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

        private void RSessionMutated(object sender, EventArgs e) {
            ReloadLoadedPackagesAsync().DoNotWait();
        }

        public void Dispose() {
            _session.Mutated -= RSessionMutated;
        }

        private enum SelectedTab {
            None,
            AvailablePackages,
            InstalledPackages,
            LoadedPackages,
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Disposables;
using Microsoft.Common.Core.Idle;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.PackageManager.Model;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Client.Session;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Editor.Functions {
    /// <summary>
    /// Index of packages available from the R engine.
    /// </summary>
    public sealed class PackageIndex : IPackageIndex {
        private readonly DisposableBag _disposableBag = new DisposableBag(nameof(PackageIndex));
        private readonly IIntellisenseRSession _host;
        private readonly IFunctionIndex _functionIndex;
        private readonly IIdleTimeService _idleTime;
        private readonly ConcurrentDictionary<string, PackageInfo> _packages = new ConcurrentDictionary<string, PackageInfo>();
        private readonly BinaryAsyncLock _buildIndexLock = new BinaryAsyncLock();
        private volatile bool _updatePending;

        public static IEnumerable<string> PreloadedPackages { get; } = new string[]
            { "base", "stats", "utils", "graphics", "datasets", "methods" };

        public PackageIndex(IServiceContainer services) : this(
            services, services.GetService<IIntellisenseRSession>(), services.GetService<IFunctionIndex>()) { }

        public PackageIndex(IServiceContainer services, IIntellisenseRSession host, IFunctionIndex functionIndex) {
            _host = host;
            _functionIndex = functionIndex;
            _idleTime = services.GetService<IIdleTimeService>();

            var interactiveWorkflowProvider = services.GetService<IRInteractiveWorkflowProvider>();
            var workflow = interactiveWorkflowProvider.GetOrCreate();

            var sessionProvider = workflow.RSessions;
            var interactiveSession = workflow.RSession;
            interactiveSession.Connected += OnSessionConnected;
            interactiveSession.PackagesInstalled += OnPackagesChanged;
            interactiveSession.PackagesRemoved += OnPackagesChanged;

            sessionProvider.BrokerStateChanged += OnBrokerStateChanged;

            if (interactiveSession.IsHostRunning) {
                BuildIndexAsync().DoNotWait();
            }

            _disposableBag
                .Add(() => interactiveSession.PackagesInstalled -= OnPackagesChanged)
                .Add(() => interactiveSession.PackagesRemoved -= OnPackagesChanged)
                .Add(() => interactiveSession.Connected -= OnSessionConnected)
                .Add(() => sessionProvider.BrokerStateChanged -= OnBrokerStateChanged)
                .Add(_host);
        }

        private void OnSessionConnected(object sender, RConnectedEventArgs e) => BuildIndexAsync().DoNotWait();

        private void OnBrokerStateChanged(object sender, BrokerStateChangedEventArgs e) {
            if (e.IsConnected) {
                UpdateInstalledPackagesAsync().DoNotWait();
            } else {
                _updatePending = true;
            }
        }

        private void OnPackagesChanged(object sender, EventArgs e) => UpdateInstalledPackagesAsync().DoNotWait();

        #region IPackageIndex
        /// <summary>
        /// Collection of all packages (base, user and project-specific)
        /// </summary>
        public IEnumerable<IPackageInfo> Packages => _packages.Values;

        public async Task BuildIndexAsync(CancellationToken ct = default(CancellationToken)) {
            var lockToken = await _buildIndexLock.WaitAsync(ct);
            await BuildIndexAsync(lockToken, ct);
        }

        private async Task BuildIndexAsync(IBinaryAsyncLockToken lockToken, CancellationToken ct) {
            try {
                if (!lockToken.IsSet) {
                    await TaskUtilities.SwitchToBackgroundThread();
                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    // Ensure session is started
                    await _host.StartSessionAsync(ct);
                    Debug.WriteLine("R function host start: {0} ms", stopwatch.ElapsedMilliseconds);

                    // Fetch list of package functions from R session
                    _disposableBag.ThrowIfDisposed();
                    stopwatch.Reset();
                    await LoadInstalledPackagesIndexAsync(ct);
                    Debug.WriteLine("Fetch list of package functions from R session: {0} ms", stopwatch.ElapsedMilliseconds);

                    // Try load missing functions from cache or explicitly
                    _disposableBag.ThrowIfDisposed();
                    stopwatch.Reset();
                    await LoadRemainingPackagesFunctions(ct);
                    Debug.WriteLine("Try load missing functions from cache or explicitly: {0} ms", stopwatch.ElapsedMilliseconds);

                    // Build index
                    _disposableBag.ThrowIfDisposed();
                    stopwatch.Reset();
                    await _functionIndex.BuildIndexAsync(this, ct);
                    Debug.WriteLine("R function index build: {0} ms", stopwatch.ElapsedMilliseconds);

                    stopwatch.Stop();
                }
            } catch (RHostDisconnectedException ex) {
                Debug.WriteLine(ex.Message);
                ScheduleIdleTimeRebuild();
            } catch (ObjectDisposedException) { }
            finally {
                lockToken.Set();
            }
        }

        /// <summary>
        /// Retrieves R package information by name. If package is not in the index,
        /// attempts to locate the package in the current R session.
        /// </summary>
        public async Task<IPackageInfo> GetPackageInfoAsync(string packageName, CancellationToken ct = default(CancellationToken)) {
            packageName = packageName.TrimQuotes().Trim();
            IPackageInfo package = GetPackageInfo(packageName);
            if (package != null) {
                return package;
            }

            Debug.WriteLine(Invariant($"Missing package: {packageName}"));
            return (await TryAddMissingPackagesAsync(new [] { packageName }, ct)).FirstOrDefault();
        }

        /// <summary>
        /// Retrieves information on multilple R packages. If one of the packages 
        /// is not in the index, attempts to locate the package in the current R session.
        /// </summary>
        public async Task<IEnumerable<IPackageInfo>> GetPackagesInfoAsync(IEnumerable<string> packageNames, CancellationToken ct = default(CancellationToken)) {
            var list = new List<IPackageInfo>();
            var missing = new List<string>();

            foreach (var n in packageNames) {
                var name = n.TrimQuotes().Trim();
                IPackageInfo package = GetPackageInfo(name);
                if (package != null) {
                    list.Add(package);
                } else {
                    Debug.WriteLine(Invariant($"Missing package: {name}"));
                    missing.Add(name);
                }
            }

            list.AddRange(await TryAddMissingPackagesAsync(missing, ct));
            return list;
        }

        public void WriteToDisk() {
            if (_buildIndexLock.IsSet) {
                foreach (var pi in _packages.Values) {
                    pi.WriteToDisk();
                }
            }
        }
        #endregion

        public void Dispose() => _disposableBag.TryDispose();

        private async Task LoadInstalledPackagesIndexAsync(CancellationToken ct) {
            var packagesFunctions = await _host.Session.InstalledPackagesFunctionsAsync(REvaluationKind.BaseEnv, ct);
            if (packagesFunctions == null) {
                return;
            }

            foreach (var package in packagesFunctions) {
                ct.ThrowIfCancellationRequested();

                var name = package.Value<string>("Package");
                var description = package.Value<string>("Description");
                var version = package.Value<string>("Version");

                var exportedFunctionNames = GetEnumerable<string>(package, "ExportedFunctions");
                var internalFunctionNames = GetEnumerable<string>(package, "InternalFunctions");

                var functions = exportedFunctionNames
                        .Select(x => new PersistentFunctionInfo(x, false))
                        .Concat(internalFunctionNames.Select(x => new PersistentFunctionInfo(x, true)));

                if (functions.Any()) {
                    _packages[name] = new PackageInfo(_host, name, description, version, functions);
                } else {
                    _packages[name] = new PackageInfo(_host, name, description, version);
                }
            }

            if (!_packages.ContainsKey("rtvs")) {
                _packages["rtvs"] = new PackageInfo(_host, "rtvs", "R Tools", "1.0");
            }
        }

        private static IEnumerable<T> GetEnumerable<T>(JToken token, string key) {
            var array = token.Value<JArray>(key);
            return array?.HasValues == true ? array.Children<JValue>().Select(v => (T)v.Value) : Enumerable.Empty<T>();
        }

        private async Task LoadRemainingPackagesFunctions(CancellationToken ct) {
            foreach (var pi in _packages.Values.Where(p => !p.Functions.Any())) {
                _disposableBag.ThrowIfDisposed();
                await pi.LoadFunctionsIndexAsync(ct);
            }
        }

        private async Task UpdateInstalledPackagesAsync(CancellationToken ct = default(CancellationToken)) {
            if (!_updatePending) {
                return;
            }
            var token = await _buildIndexLock.ResetAsync(ct);
            if (!token.IsSet) {
                try {
                    var installed = await GetInstalledPackagesAsync(ct);
                    var installedNames = installed.Select(p => p.Package).Concat(new[] { "rtvs" }).ToList();

                    var currentNames = _packages.Keys.ToArray();
                    var removedNames = currentNames.Except(installedNames);
                    _packages.RemoveWhere((kvp) => removedNames.Contains(kvp.Key));

                    var added = installed.Where(p => !currentNames.Contains(p.Package));
                    await AddPackagesToIndexAsync(added, ct);

                    _updatePending = false;
                } catch (RException) { } catch (OperationCanceledException) {
                } finally {
                    token.Reset();
                }
            }
        }

        /// <summary>
        /// From the supplied names selects packages that are not in the index and attempts
        /// to add them to the index. This typically applies to packages that were just installed.
        /// </summary>
        private async Task<IEnumerable<IPackageInfo>> TryAddMissingPackagesAsync(IEnumerable<string> packageNames, CancellationToken ct) {
            var info = Enumerable.Empty<IPackageInfo>();
            // Do not attempt to add new package when index is still being built
            if (packageNames.Any() && _buildIndexLock.IsSet) {
                try {
                    var installedPackages = await GetInstalledPackagesAsync(ct);
                    var packagesNotInIndex = installedPackages.Where(p => packageNames.Contains(p.Package));
                    info = await AddPackagesToIndexAsync(packagesNotInIndex, ct);
                } catch (RHostDisconnectedException) { }
            }
            return info;
        }

        private async Task<IEnumerable<IPackageInfo>> AddPackagesToIndexAsync(IEnumerable<RPackage> packages, CancellationToken ct) {
            var list = new List<IPackageInfo>();
            foreach (var p in packages) {
                if(ct.IsCancellationRequested) {
                    break;
                }

                var info = new PackageInfo(_host, p.Package, p.Description, p.Version);
                _packages[p.Package] = info;

                await info.LoadFunctionsIndexAsync(ct);
                _functionIndex.RegisterPackageFunctions(info);
                list.Add(info);
            }
            return list;
        }

        private async Task<IEnumerable<RPackage>> GetInstalledPackagesAsync(CancellationToken ct = default(CancellationToken)) {
            await _host.StartSessionAsync(ct);
            var result = await _host.Session.InstalledPackagesAsync();
            return result.Select(p => p.ToObject<RPackage>());
        }

        public string CacheFolderPath {
            get {
                var app = _host.Services.GetService<IPlatformServices>();
                return Path.Combine(app.ApplicationDataFolder, @"IntelliSense" + Path.DirectorySeparatorChar);
            }
        }

        public void ClearCache() {
            try {
                var fs = _host.Services.GetService<IFileSystem>();
                if (fs.DirectoryExists(CacheFolderPath)) {
                    fs.DeleteDirectory(CacheFolderPath, recursive: true);
                }
            } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }

        private void ScheduleIdleTimeRebuild() {
            IdleTimeAction.Cancel(typeof(PackageIndex));
            IdleTimeAction.Create(() => RebuildIndexAsync(CancellationToken.None).DoNotWait(), 100, GetType(), _idleTime);
        }

        private async Task RebuildIndexAsync(CancellationToken ct) {
            if (!_buildIndexLock.IsSet) {
                // Still building, try again later
                ScheduleIdleTimeRebuild();
                return;
            }

            var lockToken = await _buildIndexLock.ResetAsync(ct);
            await BuildIndexAsync(lockToken, ct);
        }

        /// <summary>
        /// Retrieves information on the package from index. Does not attempt to locate the package
        /// if it is not in the index such as when package was just installed.
        /// </summary>
        private IPackageInfo GetPackageInfo(string packageName) {
            packageName = packageName.TrimQuotes().Trim();
            _packages.TryGetValue(packageName, out PackageInfo package);
            return package;
        }

        #region IPackageInstallationNotifications
        public Task BeforePackagesInstalledAsync(CancellationToken cancellationToken) {
            // Package is about to be installed. Stop intellisense session
            // so loaded packages are released and new one will not be locked.
            // If update is pending, cancel it.
            CancelPendingIndexUpdate();
            var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(Debugger.IsAttached? 50000 : 5000);
            return _host.StopSessionAsync(timeoutCts.Token);
        }

        public Task AfterPackagesInstalledAsync(CancellationToken cancellationToken) {
            _updatePending = true;
            // Create delayed action. There may be multiple install.packages
            // commands pending and we want to update index when processing
            // is fully completes.
            IdleTimeAction.Create(() => UpdateInstalledPackagesAsync(cancellationToken).DoNotWait(), 1000, GetType(), _idleTime);
            return Task.CompletedTask;
        }
        #endregion
        private void CancelPendingIndexUpdate() {
            _updatePending = false;
            IdleTimeAction.Cancel(GetType());
        }
    }
}

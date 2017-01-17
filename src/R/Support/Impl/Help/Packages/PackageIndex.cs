// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.PackageManager.Model;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;
using Microsoft.R.Host.Client.Session;
using static System.FormattableString;

namespace Microsoft.R.Support.Help.Packages {
    /// <summary>
    /// Index of packages available from the R engine.
    /// </summary>
    [Export(typeof(IPackageIndex))]
    public sealed class PackageIndex : IPackageIndex {
        private readonly IRInteractiveWorkflow _workflow;
        private readonly IRSession _interactiveSession;
        private readonly ICoreShell _shell;
        private readonly IIntellisenseRSession _host;
        private readonly IFunctionIndex _functionIndex;
        private readonly ConcurrentDictionary<string, PackageInfo> _packages = new ConcurrentDictionary<string, PackageInfo>();
        private readonly BinaryAsyncLock _buildIndexLock = new BinaryAsyncLock();

        public static IEnumerable<string> PreloadedPackages { get; } = new string[]
            { "base", "stats", "utils", "graphics", "datasets", "methods" };

        [ImportingConstructor]
        public PackageIndex(
            IRInteractiveWorkflowProvider interactiveWorkflowProvider, ICoreShell shell, IIntellisenseRSession host, IFunctionIndex functionIndex) {
            _shell = shell;
            _host = host;
            _functionIndex = functionIndex;

            _workflow = interactiveWorkflowProvider.GetOrCreate();

            _interactiveSession = _workflow.RSession;
            _interactiveSession.Connected += OnSessionConnected;
            _interactiveSession.PackagesInstalled += OnPackagesChanged;
            _interactiveSession.PackagesRemoved += OnPackagesChanged;

            _workflow.RSessions.BrokerStateChanged += OnBrokerStateChanged;

            if (_workflow.RSession.IsHostRunning) {
                BuildIndexAsync().DoNotWait();
            }
        }

        private void OnSessionConnected(object sender, RConnectedEventArgs e) {
            BuildIndexAsync().DoNotWait();
        }

        private void OnBrokerStateChanged(object sender, BrokerStateChangedEventArgs e) {
            if (e.IsConnected) {
                BuildIndexAsync().DoNotWait();
            }
        }

        private void OnPackagesChanged(object sender, EventArgs e) {
            _buildIndexLock.EnqueueReset();
            ScheduleIdleTimeRebuild();
        }

        #region IPackageIndex
        /// <summary>
        /// Collection of all packages (base, user and project-specific)
        /// </summary>
        public IEnumerable<IPackageInfo> Packages => _packages.Values;

        public async Task BuildIndexAsync() {
            var lockToken = await _buildIndexLock.WaitAsync();
            await BuildIndexAsync(lockToken);
        }

        private async Task BuildIndexAsync(IBinaryAsyncLockToken lockToken) {
            try {
                if (!lockToken.IsSet) {
                    var startTotalTime = DateTime.Now;

                    await TaskUtilities.SwitchToBackgroundThread();
                    await _host.StartSessionAsync();
                    Debug.WriteLine("R function host start: {0} ms", (DateTime.Now - startTotalTime).TotalMilliseconds);

                    var startTime = DateTime.Now;
                    // Fetch list of available packages from R session
                    await BuildInstalledPackagesIndexAsync();
                    Debug.WriteLine("R package names/description: {0} ms", (DateTime.Now - startTime).TotalMilliseconds);

                    // Populate function index for preloaded packages first
                    startTime = DateTime.Now;
                    await BuildPreloadedPackagesFunctionListAsync();
                    Debug.WriteLine("R function index (preloaded): {0} ms", (DateTime.Now - startTime).TotalMilliseconds);

                    // Populate function index for all remaining packages
                    startTime = DateTime.Now;
                    await BuildRemainingPackagesFunctionListAsync();
                    Debug.WriteLine("R function index (remaining): {0} ms", (DateTime.Now - startTime).TotalMilliseconds);

                    await _functionIndex.BuildIndexAsync(this);
                    Debug.WriteLine("R function index total: {0} ms", (DateTime.Now - startTotalTime).TotalMilliseconds);
                }
            } catch (RHostDisconnectedException ex) {
                Debug.WriteLine(ex.Message);
                ScheduleIdleTimeRebuild();
            } finally {
                lockToken.Set();
            }
        }

        /// <summary>
        /// Retrieves R package information by name. If package is not in the index,
        /// attempts to locate the package in the current R session.
        /// </summary>
        public async Task<IPackageInfo> GetPackageInfoAsync(string packageName) {
            packageName = packageName.TrimQuotes().Trim();
            IPackageInfo package = GetPackageInfo(packageName);
            if (package != null) {
                return package;
            }

            Debug.WriteLine(Invariant($"Missing package: {packageName}"));
            return (await TryAddMissingPackagesAsync(new string[] { packageName })).FirstOrDefault();
        }

        /// <summary>
        /// Retrieves information on multilple R packages. If one of the packages 
        /// is not in the index, attempts to locate the package in the current R session.
        /// </summary>
        public async Task<IEnumerable<IPackageInfo>> GetPackagesInfoAsync(IEnumerable<string> packageNames) {
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

            list.AddRange(await TryAddMissingPackagesAsync(missing));
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

        public void Dispose() {
            if (_interactiveSession != null) {
                _interactiveSession.PackagesInstalled -= OnPackagesChanged;
                _interactiveSession.PackagesRemoved -= OnPackagesChanged;
                _interactiveSession.Connected -= OnSessionConnected;
                _workflow.RSessions.BrokerStateChanged -= OnBrokerStateChanged;
            }
            _host?.Dispose();
        }

        private async Task BuildInstalledPackagesIndexAsync() {
            var packages = await GetInstalledPackagesAsync();
            foreach (var p in packages) {
                _packages[p.Package] = new PackageInfo(_host, p.Package, p.Description, p.Version);
            }
            _packages["rtvs"] = new PackageInfo(_host, "rtvs", "R Tools", "1.0");
        }

        private async Task BuildPreloadedPackagesFunctionListAsync() {
            foreach (var packageName in PreloadedPackages) {
                PackageInfo pi;
                _packages.TryGetValue(packageName, out pi);
                if (pi != null) {
                    await pi.LoadFunctionsIndexAsync();
                }
            }
        }

        private async Task BuildRemainingPackagesFunctionListAsync() {
            foreach (var pi in _packages.Values) {
                if (!pi.Functions.Any()) {
                    await pi.LoadFunctionsIndexAsync();
                }
            }
        }

        /// <summary>
        /// From the supplied names selects packages that are not in the index and attempts
        /// to add them to the index. This typically applies to packages that were just installed.
        /// </summary>
        private async Task<IEnumerable<IPackageInfo>> TryAddMissingPackagesAsync(IEnumerable<string> packageNames) {
            var list = new List<IPackageInfo>();
            // Do not attempt to add new package when index is still being built
            if (packageNames.Any() && _buildIndexLock.IsSet) {
                try {
                    var installedPackages = await GetInstalledPackagesAsync();
                    var packagesNotInIndex = installedPackages.Where(p => packageNames.Contains(p.Package));
                    foreach (var p in packagesNotInIndex) {
                        var info = new PackageInfo(_host, p.Package, p.Description, p.Version);
                        _packages[p.Package] = info;

                        await info.LoadFunctionsIndexAsync();
                        _functionIndex.RegisterPackageFunctions(info);

                        list.Add(info);
                    }
                } catch (RHostDisconnectedException) { }
            }
            return list;
        }

        private async Task<IEnumerable<RPackage>> GetInstalledPackagesAsync() {
            try {
                await _host.StartSessionAsync();
                var result = await _host.Session.InstalledPackagesAsync();
                return result.Select(p => p.ToObject<RPackage>());
            } catch (TaskCanceledException) { }
            return Enumerable.Empty<RPackage>();
        }

        private async Task<IEnumerable<string>> GetLoadedPackagesAsync() {
            try {
                await _host.StartSessionAsync();
                return _host.LoadedPackageNames;
            } catch (OperationCanceledException) { }
            return Enumerable.Empty<string>();
        }

        internal static string CacheFolderPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\VisualStudio\RTVS\IntelliSense\");

        public static void ClearCache() {
            try {
                if (Directory.Exists(CacheFolderPath)) {
                    Directory.Delete(CacheFolderPath, recursive: true);
                }
            } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }

        private void ScheduleIdleTimeRebuild() {
            IdleTimeAction.Cancel(typeof(PackageIndex));
            IdleTimeAction.Create(() => RebuildIndexAsync().DoNotWait(), 100, typeof(PackageIndex), _shell);
        }

        private async Task RebuildIndexAsync() {
            if (!_buildIndexLock.IsSet) {
                // Still building, try again later
                ScheduleIdleTimeRebuild();
                return;
            }

            var lockToken = await _buildIndexLock.ResetAsync();
            await BuildIndexAsync(lockToken);
        }

        /// <summary>
        /// Retrieves information on the package from index. Does not attempt to locate the package
        /// if it is not in the index such as when package was just installed.
        /// </summary>
        private IPackageInfo GetPackageInfo(string packageName) {
            PackageInfo package = null;
            packageName = packageName.TrimQuotes().Trim();
            _packages.TryGetValue(packageName, out package);
            return package;
        }
    }
}

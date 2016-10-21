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
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Support.Help.Packages {
    /// <summary>
    /// Index of packages available from the R engine.
    /// </summary>
    [Export(typeof(IPackageIndex))]
    public sealed class PackageIndex : IPackageIndex {
        private readonly IRSession _interactiveSession;
        private readonly ICoreShell _shell;
        private readonly IIntellisenseRSession _host;
        private readonly IFunctionIndex _functionIndex;
        private readonly ConcurrentDictionary<string, PackageInfo> _packages = new ConcurrentDictionary<string, PackageInfo>();
        private readonly BinaryAsyncLock _buildIndexLock = new BinaryAsyncLock();

        public static IEnumerable<string> PreloadedPackages { get; } = new string[]
            { "base", "stats", "utils", "graphics", "datasets", "methods" };

        [ImportingConstructor]
        public PackageIndex(IRInteractiveWorkflowProvider interactiveWorkflowProvider, ICoreShell shell, IIntellisenseRSession host, IFunctionIndex functionIndex) {
            _shell = shell;
            _host = host;
            _functionIndex = functionIndex;

            _interactiveSession = interactiveWorkflowProvider.GetOrCreate().RSession;

            _interactiveSession.PackagesInstalled += OnPackagesChanged;
            _interactiveSession.PackagesRemoved += OnPackagesChanged;
        }

        private void OnPackagesChanged(object sender, EventArgs e) => ScheduleIdleTimeRebuild();

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
                    await _host.CreateSessionAsync();
                    Debug.WriteLine("R function host start: {0} ms", (DateTime.Now - startTotalTime).TotalMilliseconds);

                    var startTime = DateTime.Now;
                    // Fetch list of available packages from R session
                    await BuildPackageListAsync();
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
            } finally {
                lockToken.Set();
            }
        }

        /// <summary>
        /// Retrieves information on the package from index. Does not attempt to locate the package
        /// if it is not in the index such as when package was just installed.
        /// </summary>
        public IPackageInfo GetPackageInfo(string packageName) {
            PackageInfo package = null;
            packageName = packageName.TrimQuotes().Trim();
            _packages.TryGetValue(packageName, out package);
            return package;
        }

        /// <summary>
        /// Retrieves R package information by name. If package is not in the index,
        /// attempts to locate the package in the current R session.
        /// </summary>
        public Task<IPackageInfo> GetPackageInfoAsync(string packageName) {
            packageName = packageName.TrimQuotes().Trim();
            IPackageInfo package = GetPackageInfo(packageName);
            if (package == null) {
                return TryAddNewPackageAsync(packageName);
            }
            return Task.FromResult(package);
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
            }
            _host?.Dispose();
        }

        private async Task BuildPackageListAsync() {
            var packages = await GetPackagesAsync();
            foreach (var p in packages) {
                _packages[p.Package] = new PackageInfo(_host, p.Package, p.Description, p.Version);
            }
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

        private async Task<IPackageInfo> TryAddNewPackageAsync(string packageName) {
            PackageInfo packageInfo = null;
            try {
                if (packageName.EqualsOrdinal("rtvs")) {
                    packageInfo = new PackageInfo(_host, packageName, "R Tools", "1.0");
                } else {
                    var packages = await GetPackagesAsync();
                    var package = packages.FirstOrDefault(p => p.Package.EqualsOrdinal(packageName));
                    if (package != null) {
                        packageInfo = new PackageInfo(_host, package.Package, package.Description, package.Version);
                    }
                }

                if (packageInfo != null) {
                    await packageInfo.LoadFunctionsIndexAsync();
                    _packages[packageName] = packageInfo;
                    _functionIndex.RegisterPackageFunctions(packageInfo);
                }
            } catch (RHostDisconnectedException) { }
            return packageInfo;
        }

        private async Task<IEnumerable<RPackage>> GetPackagesAsync() {
            try {
                await _host.CreateSessionAsync();
                var result = await _host.Session.InstalledPackagesAsync();
                return result.Select(p => p.ToObject<RPackage>());
            } catch (TaskCanceledException) { }
            return Enumerable.Empty<RPackage>();
        }

        internal static string CacheFolderPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Microsoft\VisualStudio\RTVS\IntelliSense\");

        public static void ClearCache() {
            try {
                Directory.Delete(CacheFolderPath, recursive: true);
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
    }
}

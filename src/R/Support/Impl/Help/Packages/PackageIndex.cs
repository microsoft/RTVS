// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Components.PackageManager.Model;
using Microsoft.R.Host.Client;
using Newtonsoft.Json.Linq;
using static System.FormattableString;

namespace Microsoft.R.Support.Help.Packages {
    /// <summary>
    /// Index of packages available from the R engine.
    /// </summary>
    [Export(typeof(IPackageIndex))]
    internal sealed class PackageIndex : IPackageIndex {
        private readonly ICoreShell _shell;
        private readonly BinaryAsyncLock _lock = new BinaryAsyncLock();
        private readonly ConcurrentDictionary<string, IPackageInfo> _packages = new ConcurrentDictionary<string, IPackageInfo>();
        private IRSession _session;
        private IFunctionIndex _functionIndex;

        [ImportingConstructor]
        public PackageIndex(ICoreShell shell) {
            // TODO: connect to installed/uninstalled/loaded/unloaded events from 
            // the package manager when it gets them implemented.
            _shell = shell;
        }

        /// <summary>
        /// Collection of all packages (base, user and project-specific)
        /// </summary>
        public IEnumerable<IPackageInfo> Packages => _packages.Values;

        public async Task BuildIndexAsync(IFunctionIndex functionIndex, IRSession session) {
            _functionIndex = functionIndex;
            _session = session;

            await TaskUtilities.SwitchToBackgroundThread();

            var startTime = DateTime.Now;
            var packages = await GetPackagesAsync();
            foreach (var p in packages) {
                _packages[p.Package] = new PackageInfo(functionIndex, p.Package, p.Description);
            }
            Debug.WriteLine("R package names/description: {0} ms", (DateTime.Now - startTime).TotalMilliseconds);
        }

        /// <summary>
        /// Collection of all packages (base, user and project-specific)
        /// </summary>
        public Task<IPackageInfo> GetPackageByNameAsync(string packageName) {
            IPackageInfo package = null;
            packageName = packageName.TrimQuotes().Trim();
            _packages.TryGetValue(packageName, out package);
            if (package == null) {
                return TryAddNewPackageAsync(packageName);
            }
            return Task.FromResult(package);
        }

        private async Task<IPackageInfo> TryAddNewPackageAsync(string packageName) {
            if (_functionIndex != null) {
                var packages = await GetPackagesAsync();
                var package = packages.FirstOrDefault(p => p.Package.EqualsOrdinal(packageName));
                if (package != null) {
                    var p = new PackageInfo(_functionIndex, package.Package, package.Description);
                    _packages[packageName] = p;
                    await _functionIndex.AddFunctionsFromPackage(packageName);
                    return p;
                }
            }
            return null;
        }

        private async Task<IEnumerable<RPackage>> GetPackagesAsync() {
            try {
                var result = await _session.EvaluateAsync<JArray>(Invariant($"rtvs:::packages.installed()"), REvaluationKind.Normal);
                return result.Select(p => p.ToObject<RPackage>());
            } catch (MessageTransportException) { } catch (TaskCanceledException) { }
            return Enumerable.Empty<RPackage>();
        }
    }
}

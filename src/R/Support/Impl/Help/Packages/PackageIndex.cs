// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.Languages.Core.Formatting;
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
        private readonly Dictionary<string, IPackageInfo> _packages = new Dictionary<string, IPackageInfo>();
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

        public async Task BuildIndexAsync(IFunctionIndex functionIndex) {
            _functionIndex = functionIndex;
            await TaskUtilities.SwitchToBackgroundThread();
            var packageNames = await GetPackageNamesAsync();
            foreach (var name in packageNames) {
                var description = await GetPackageDescriptionAsync(name);
                _packages[name] = new PackageInfo(functionIndex, name, description);
            }
        }

        /// <summary>
        /// Collection of all packages (base, user and project-specific)
        /// </summary>
        public IPackageInfo GetPackageByName(string packageName) {
            IPackageInfo package = null;
            packageName = packageName.TrimQuotes().Trim();
            _packages.TryGetValue(packageName, out package);
            if (package == null) {
                var task = TryAddNewPackageAsync(packageName);
                task.Wait(1000);
                if (task.IsCompleted) {
                    _packages.TryGetValue(packageName, out package);
                }
            }
            return package;
        }

        private Task TryAddNewPackageAsync(string packageName) {
            if (_functionIndex != null) {
                return Task.Run(async () => {
                    var packageNames = await GetPackageNamesAsync();
                    var name = packageNames.FirstOrDefault(x => x.EqualsOrdinal(packageName));
                    if (name != null) {
                        var description = await GetPackageDescriptionAsync(name);
                        var p = new PackageInfo(_functionIndex, name, description);
                        _packages[packageName] = p;
                    }
                });
            }
            return Task.CompletedTask;
        }

        private async Task<IEnumerable<string>> GetPackageNamesAsync() {
            try {
                var r = await _functionIndex.RSession.EvaluateAsync<JArray>(Invariant($"as.list(.packages(all = TRUE))"), REvaluationKind.Normal);
                return r.Select(p => (string)((JValue)p).Value).ToArray();
            } catch (MessageTransportException) { } catch (TaskCanceledException) { }
            return Enumerable.Empty<string>();
        }

        private async Task<string> GetPackageDescriptionAsync(string name) {
            try {
                var t = await _functionIndex.RSession.EvaluateAsync<JToken>(Invariant($"utils::packageDescription('{name}', fields = 'Description')"), REvaluationKind.Normal);
                var value = t.Value<string>();
                return value != null ? value.NormalizeWhitespace() : string.Empty;
            } catch (MessageTransportException) { } catch (TaskCanceledException) { }
            return string.Empty;
        }
    }
}

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
using Microsoft.R.Support.RD.Parser;
using static System.FormattableString;

namespace Microsoft.R.Support.Help.Functions {
    /// <summary>
    /// Provides information on functions in packages for intellisense.
    /// </summary>
    [Export(typeof(IFunctionIndex))]
    public sealed class FunctionIndex : IFunctionIndex {
        private readonly ICoreShell _coreShell;
        private readonly IIntellisenseRSession _host;
        private readonly BinaryAsyncLock _buildIndexLock = new BinaryAsyncLock();

        /// <summary>
        /// Map of functions to packages. Used to quickly find package 
        /// name by function name as we need both to get the function 
        /// documentation in RD format from the R engine.
        /// </summary>
        private readonly ConcurrentDictionary<string, List<string>> _functionToPackageMap = new ConcurrentDictionary<string, List<string>>();

        /// <summary>
        /// Map of function names to complete function information.
        /// Used to construct and show quick info tooltips as well
        /// as help and completion of the function signature.
        /// </summary>
        private readonly ConcurrentDictionary<string, IFunctionInfo> _functionToInfoMap = new ConcurrentDictionary<string, IFunctionInfo>();

        /// <summary>
        /// Provides RD data (help) on a function from the specified R package.
        /// Typically exported via MEF from the host that runs R.dll.
        /// </summary>
        private readonly IFunctionRdDataProvider _functionRdDataProvider;

        [ImportingConstructor]
        public FunctionIndex(ICoreShell coreShell, IFunctionRdDataProvider rdDataProfider, IIntellisenseRSession host) {
            _coreShell = coreShell;
            _functionRdDataProvider = rdDataProfider;
            _host = host;
        }

        public async Task BuildIndexAsync(IPackageIndex packageIndex = null) {
            packageIndex = packageIndex ?? _coreShell.ExportProvider.GetExportedValue<IPackageIndex>();
            var lockToken = await _buildIndexLock.WaitAsync();
            try {
                if (!lockToken.IsSet) {
                    // First populate index for popular packages that are commonly preloaded
                    foreach (var pi in packageIndex.Packages) {
                        foreach (var f in pi.Functions) {
                            RegisterFunction(f.Name, pi.Name);
                        }
                    }
                }
            } finally {
                lockToken.Set();
            }
        }

        private void RegisterFunction(string functionName, string packageName) {
            List<string> packages;
            if (!_functionToPackageMap.TryGetValue(functionName, out packages)) {
                _functionToPackageMap[functionName] = new List<string>() { packageName };
            } else {
                packages.Add(packageName);
            }
        }

        public void RegisterPackageFunctions(IPackageInfo package) {
            foreach (var f in package.Functions) {
                RegisterFunction(f.Name, package.Name);
            }
        }

        /// <summary>
        /// Retrieves function information by name. If information is not
        /// available, starts asynchronous retrieval of the function info
        /// from R and when the data becomes available invokes specified
        /// callback passing the parameter. This is used for async
        /// intellisense or function signature/parameter help.
        /// </summary>
        public IFunctionInfo GetFunctionInfo(string functionName, string packageName, Action<object, string> infoReadyCallback = null, object parameter = null) {
            var functionInfo = TryGetCachedFunctionInfo(functionName, ref packageName);
            if (functionInfo != null) {
                return functionInfo;
            }
            GetFunctionInfoFromEngineAsync(functionName, packageName, infoReadyCallback, parameter).DoNotWait();
            return null;
        }

        public async Task<IFunctionInfo> GetFunctionInfoAsync(string functionName, string packageName = null) {
            var functionInfo = TryGetCachedFunctionInfo(functionName, ref packageName);
            if (functionInfo != null) {
                return functionInfo;
            }
            packageName = await GetFunctionInfoFromEngineAsync(functionName, packageName);
            return await TryGetCachedFunctionInfoAsync(functionName, packageName);
        }

        /// <summary>
        /// Attempts to retrieve function information from cache is a simple manner.
        /// Specifically, when function name is unique (then package name is irrelevant)
        /// or the package name is known.
        /// </summary>
        private IFunctionInfo TryGetCachedFunctionInfo(string functionName, ref string packageName) {
            IFunctionInfo functionInfo = null;
            if (string.IsNullOrEmpty(packageName)) {
                // Find packages that the function may belong to. There may be more than one.
                List<string> packages;
                if (!_functionToPackageMap.TryGetValue(functionName, out packages) || packages.Count == 0) {
                    // Not in the cache
                    return null;
                }

                // Special case RTVS package
                if (packages.Count == 1 && packages[0].EqualsOrdinal("rtvs")) {
                    packageName = packages[0];
                } else {
                    // If there is only one package, try it. 
                    var loaded = _host.LoadedPackageNames.Intersect(packages).ToArray();
                    if (loaded.Length == 1) {
                        packageName = loaded[0];
                    }
                }
            } else if (!packageName.EqualsOrdinal("rtvs") && !_host.LoadedPackageNames.Contains(packageName)) {
                // Verify that the package is currently loaded. We do not show functions from all
                // installed packages and do not show from unloaded packages.
                return null;
            }

            if (!string.IsNullOrEmpty(packageName)) {
                _functionToInfoMap?.TryGetValue(GetQualifiedName(functionName, packageName), out functionInfo);
            }
            return functionInfo;
        }

        /// <summary>
        /// Attempts to retrieve function information from cache is a simple manner.
        /// Specifically, when function name is unique (then package name is irrelevant)
        /// or the package name is known.
        /// </summary>
        private async Task<IFunctionInfo> TryGetCachedFunctionInfoAsync(string functionName, string packageName) {
            if (packageName == null) {
                packageName = await GetFunctionLoadedPackage(functionName);
            }
            return TryGetCachedFunctionInfo(functionName, ref packageName);
        }

        private string GetQualifiedName(string functionName, string packageName) {
            return Invariant($"{packageName}:::{functionName}");
        }

        /// <summary>
        /// Fetches help on the function from R asynchronously.
        /// When function data is obtained, parsed and the function
        /// index is updated, method invokes <see cref="infoReadyCallback"/>
        /// callback passing the specified parameter. Callback method can now
        /// fetch function information from the index.
        /// </summary>
        private async Task<string> GetFunctionInfoFromEngineAsync(string functionName, string packageName, Action<object, string> infoReadyCallback = null, object parameter = null) {
            packageName = packageName ?? await _host.GetFunctionPackageNameAsync(functionName);

            if (string.IsNullOrEmpty(packageName)) {
                // Even if nothing is found, still notify the callback
                if (infoReadyCallback != null) {
                    _coreShell.DispatchOnUIThread(() => {
                        infoReadyCallback(null, null);
                    });
                }
                return packageName;
            }

            if (infoReadyCallback == null) {
                // regular async call
                var rdData = await _functionRdDataProvider.GetFunctionRdDataAsync(functionName, packageName);
                if (!string.IsNullOrEmpty(rdData)) {
                    // If package is found update data in the index
                    UpdateIndex(functionName, packageName, rdData);
                }
                return packageName;
            }

            _functionRdDataProvider.GetFunctionRdDataAsync(functionName, packageName,
                rdData => {
                    if (!string.IsNullOrEmpty(packageName) && !string.IsNullOrEmpty(rdData)) {
                        // If package is found update data in the index
                        UpdateIndex(functionName, packageName, rdData);
                    }
                    _coreShell.DispatchOnUIThread(() => infoReadyCallback(parameter, packageName));
                });
            return packageName;
        }

        private async Task<string> GetFunctionLoadedPackage(string functionName) {
            var packageName = await _host.GetFunctionPackageNameAsync(functionName);
            if (!string.IsNullOrEmpty(packageName) && _host.LoadedPackageNames.Contains(packageName)) {
                return packageName;
            }
            return null;
        }

        private void UpdateIndex(string functionName, string packageName, string rdData) {
            IReadOnlyList<IFunctionInfo> functionInfos = GetFunctionInfosFromRd(rdData);

            foreach (IFunctionInfo info in functionInfos) {
                _functionToInfoMap[GetQualifiedName(info.Name, packageName)] = info;
            }

            var qualifiedName = GetQualifiedName(functionName, packageName);
            if (!_functionToInfoMap.ContainsKey(qualifiedName)) {
                if (functionInfos.Count > 0) {
                    // RD doesn't contain the requested function.
                    // e.g. as.Date.character has RD for as.Date but not itself
                    // without its own named info, this will request indefinitely many times
                    // as workaround, add the first info with functionName
                    _functionToInfoMap[qualifiedName] = functionInfos[0];
                } else {
                    // Add stub function info here to prevent subsequent calls
                    // for the same function as we already know the call will fail.
                    _functionToInfoMap[qualifiedName] = new FunctionInfo(functionName);
                }
            }
        }

        private IReadOnlyList<IFunctionInfo> GetFunctionInfosFromRd(string rdData) {
            IReadOnlyList<IFunctionInfo> infos = null;
            try {
                infos = RdParser.GetFunctionInfos(rdData);
            } catch (Exception ex) {
                Debug.WriteLine("Exception in parsing R engine RD response: {0}", ex.Message);
            }
            return infos;
        }
    }
}

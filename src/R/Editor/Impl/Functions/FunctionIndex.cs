// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Core.Tokens;
using Microsoft.R.Editor.RData.Parser;
using static System.FormattableString;

namespace Microsoft.R.Editor.Functions {
    /// <summary>
    /// Provides information on functions in packages for intellisense.
    /// </summary>
    public sealed class FunctionIndex : IFunctionIndex {
        private readonly IIntellisenseRSession _host;
        private readonly BinaryAsyncLock _buildIndexLock = new BinaryAsyncLock();

        /// <summary>
        /// Map of exported functions to packages. Used to quickly find package 
        /// name by function name as we need both to get the function 
        /// documentation in RD format from the R engine.
        /// </summary>
        private readonly ConcurrentDictionary<string, List<string>> _exportedFunctionToPackageMap = new ConcurrentDictionary<string, List<string>>();

        /// <summary>
        /// Map of internal functions to packages. Used to quickly find package 
        /// name by function name as we need both to get the function 
        /// documentation in RD format from the R engine.
        /// </summary>
        private readonly ConcurrentDictionary<string, List<string>> _internalFunctionToPackageMap = new ConcurrentDictionary<string, List<string>>();

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

        public FunctionIndex(IServiceContainer services) :
            this(services, services.GetService<IFunctionRdDataProvider>(), services.GetService<IIntellisenseRSession>()) { }

        public FunctionIndex(IServiceContainer services, IFunctionRdDataProvider rdDataProfider, IIntellisenseRSession host) {
            Services = services;
            _functionRdDataProvider = rdDataProfider;
            _host = host;
        }

        /// <summary>
        /// Provides access to services in extension methods
        /// </summary>
        public IServiceContainer Services { get; }

        /// <summary>
        /// Builds function index
        /// </summary>
        /// <param name="packageIndex">Package index, if available. If not available, 
        /// index builder will attempt to obtain it from the service container</param>
        public async Task BuildIndexAsync(IPackageIndex packageIndex = null, CancellationToken ct = default(CancellationToken)) {
            packageIndex = packageIndex ?? Services.GetService<IPackageIndex>();
            var lockToken = await _buildIndexLock.WaitAsync(ct);
            try {
                if (!lockToken.IsSet) {
                    // First populate index for popular packages that are commonly preloaded
                    foreach (var pi in packageIndex.Packages) {
                        if (ct.IsCancellationRequested) {
                            break;
                        }
                        foreach (var f in pi.Functions) {
                            if (ct.IsCancellationRequested) {
                                break;
                            }
                            RegisterFunction(f.Name, pi.Name, f.IsInternal);
                        }
                    }
                }
            } finally {
                lockToken.Set();
            }
        }

        private void RegisterFunction(string functionName, string packageName, bool isInternal) {
            if (isInternal) {
                if (!_internalFunctionToPackageMap.TryGetValue(functionName, out List<string> packages)) {
                    _internalFunctionToPackageMap[functionName] = new List<string> {packageName};
                } else {
                    packages.Add(packageName);
                }
            } else {
                if (!_exportedFunctionToPackageMap.TryGetValue(functionName, out List<string> packages)) {
                    _exportedFunctionToPackageMap[functionName] = new List<string> { packageName };
                } else {
                    packages.Add(packageName);
                }
            }
        }

        public void RegisterPackageFunctions(IPackageInfo package) {
            foreach (var f in package.Functions) {
                RegisterFunction(f.Name, package.Name, f.IsInternal);
            }
        }

        #region IFunctionIndex
        /// <summary>
        /// Attempts to retrieve cached information.
        /// </summary>
        /// <param name="functionName">Function name</param>
        /// <param name="packageName">Package name if available</param>
        /// <returns>Function information or null if not found.</returns>
        public IFunctionInfo GetFunctionInfo(string functionName, string packageName = null)
            => TryGetCachedFunctionInfo(functionName, ref packageName);

        /// <summary>
        /// Attempts to locate and cache function information. When it completes
        /// it is possible to call <see cref="GetFunctionInfo"/> 
        /// right away and get the function information.
        /// </summary>
        /// <param name="functionName">Function name</param>
        /// <param name="packageName">Package name</param>
        /// <returns></returns>
        public Task<IFunctionInfo> GetFunctionInfoAsync(string functionName, string packageName = null)
            => GetFunctionInfoFromEngineAsync(functionName, packageName);

        /// <summary>
        /// Attempts to determine package the function belongs to. Package name depends on the order of loading.
        /// For example, 'select' may be from 'MASS' or from 'dplyr' depending which package was loaded last.
        /// The function also retrieves and caches function information so it is possible to call
        /// <see cref="GetFunctionInfo"/> right away and get the function information.
        /// </summary>
        /// <param name="functionName">Name of the function</param>
        /// <returns>Name of the package</returns>
        public async Task<string> GetPackageNameAsync(string functionName) {
            var fi = await GetFunctionInfoFromEngineAsync(functionName, null);
            fi = await TryGetCachedFunctionInfoAsync(functionName, fi?.Package);
            return fi?.Package;
        }
        #endregion

        /// <summary>
        /// Attempts to retrieve function information from cache is a simple manner.
        /// Specifically, when function name is unique (then package name is irrelevant)
        /// or the package name is known.
        /// </summary>
        private IFunctionInfo TryGetCachedFunctionInfo(string functionName, ref string packageName) {
            IFunctionInfo functionInfo = null;
            if (string.IsNullOrEmpty(packageName)) {
                // Find packages that the function may belong to. There may be more than one.
                if (!_exportedFunctionToPackageMap.TryGetValue(functionName, out var packages)) {
                    if (!_internalFunctionToPackageMap.TryGetValue(functionName, out packages)) {
                        return null;
                    }
                }
                if (packages.Count == 0) {
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
            packageName = packageName ?? await GetFunctionLoadedPackage(functionName);
            await _host.GetLoadedPackageNamesAsync(); // Make sure the list is up to date
            return TryGetCachedFunctionInfo(functionName, ref packageName);
        }

        private static string GetQualifiedName(string functionName, string packageName) => Invariant($"{packageName}:::{functionName}");

        /// <summary>
        /// Fetches help on the function from R asynchronously.
        /// </summary>
        private async Task<IFunctionInfo> GetFunctionInfoFromEngineAsync(string functionName, string packageName) {
            if (!IsValidFunctionName(functionName)) {
                return null;
            }

            // Make sure loaded packages collection is up to date
            await _host.GetLoadedPackageNamesAsync();
            TryGetCachedFunctionInfo(functionName, ref packageName);

            packageName = packageName ?? await _host.GetFunctionPackageNameAsync(functionName);
            if (string.IsNullOrEmpty(packageName)) {
                return null;
            }

            var rdData = await _functionRdDataProvider.GetFunctionRdDataAsync(functionName, packageName);
            if (!string.IsNullOrEmpty(rdData)) {
                // If package is found update data in the index
                UpdateIndex(functionName, packageName, rdData);
                return TryGetCachedFunctionInfo(functionName, ref packageName);
            }
            return null;
        }

        private static bool IsValidFunctionName(string functionName) {
            var tokens = new RTokenizer().Tokenize(functionName);
            return tokens.Count == 1 && tokens[0].TokenType == RTokenType.Identifier;
        }

        /// <summary>
        /// Given function name returns loaded package the function belongs to.
        /// The package is determined from the interactive R session since
        /// there may be functions with the same name but from different packages.
        /// Most recently loaded package typically wins.
        /// </summary>
        private async Task<string> GetFunctionLoadedPackage(string functionName) {
            var packageName = await _host.GetFunctionPackageNameAsync(functionName);
            if (!string.IsNullOrEmpty(packageName) && (await _host.GetLoadedPackageNamesAsync()).Contains(packageName)) {
                return packageName;
            }
            return null;
        }

        private void UpdateIndex(string functionName, string packageName, string rdData) {
            var functionInfos = GetFunctionInfosFromRd(packageName, rdData);
            if (functionInfos == null) {
                return;
            }

            foreach (var info in functionInfos) {
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
                    _functionToInfoMap[qualifiedName] = new FunctionInfo(functionName, true);
                }
            }
        }

        private static IReadOnlyList<IFunctionInfo> GetFunctionInfosFromRd(string packageName, string rdData) {
            IReadOnlyList<IFunctionInfo> infos = null;
            try {
                infos = RdParser.GetFunctionInfos(packageName, rdData);
            } catch (Exception ex) {
                Debug.WriteLine("Exception in parsing R engine RD response: {0}", ex.Message);
            }
            return infos;
        }
    }
}

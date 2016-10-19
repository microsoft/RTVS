// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Threading;
using Microsoft.R.Support.RD.Parser;

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
        private readonly ConcurrentDictionary<string, string> _functionToPackageMap = new ConcurrentDictionary<string, string>();

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
                            // Avoid duplicates. Packages are normally populated from base (intrinsic) 
                            // to user so we don't want new packages to override base function information
                            if (!_functionToPackageMap.ContainsKey(f.Name)) {
                                _functionToPackageMap[f.Name] = pi.Name;
                            }
                        }
                    }
                }
            } finally {
                lockToken.Set();
            }
        }

        public void RegisterPackageFunctions(IPackageInfo package) {
            foreach(var f in package.Functions) {
                _functionToPackageMap[f.Name] = package.Name;
            }
        }

        /// <summary>
        /// Retrieves function information by name. If information is not
        /// available, starts asynchronous retrieval of the function info
        /// from R and when the data becomes available invokes specified
        /// callback passing the parameter. This is used for async
        /// intellisense or function signature/parameter help.
        /// </summary>
        public IFunctionInfo GetFunctionInfo(string functionName, Action<object> infoReadyCallback = null, object parameter = null) {
            var functionInfo = TryGetCachedFunctionInfo(functionName);
            if (functionInfo == null) {
                string packageName;
                if (_functionToPackageMap.TryGetValue(functionName, out packageName)) {
                    GetFunctionInfoFromEngineAsync(functionName, packageName, infoReadyCallback, parameter);
                }
            }
            return functionInfo;
        }

        public async Task<IFunctionInfo> GetFunctionInfoAsync(string functionName) {
            var functionInfo = TryGetCachedFunctionInfo(functionName);
            if (functionInfo == null) {
                string packageName;
                if (_functionToPackageMap.TryGetValue(functionName, out packageName)) {
                    return await GetFunctionInfoFromEngineAsync(functionName, packageName);
                }
            }
            return functionInfo;
        }

        private IFunctionInfo TryGetCachedFunctionInfo(string functionName) {
            IFunctionInfo functionInfo = null;
            _functionToInfoMap?.TryGetValue(functionName, out functionInfo);
            return functionInfo;
        }

        /// <summary>
        /// Fetches help on the function from R asynchronously.
        /// When function data is obtained, parsed and the function
        /// index is updated, method invokes <see cref="infoReadyCallback"/>
        /// callback passing the specified parameter. Callback method can now
        /// fetch function information from the index.
        /// </summary>
        private void GetFunctionInfoFromEngineAsync(string functionName, string packageName, Action<object> infoReadyCallback = null, object parameter = null) {
            _functionRdDataProvider.GetFunctionRdDataAsync(functionName, packageName,
                rdData => {
                    UpdateIndex(functionName, rdData);
                    if (infoReadyCallback != null) {
                        _coreShell.DispatchOnUIThread(() => {
                            infoReadyCallback(parameter);
                        });
                    }
                });
        }

        /// <summary>
        /// Fetches help on the function from R asynchronously.
        /// </summary>
        public async Task<IFunctionInfo> GetFunctionInfoFromEngineAsync(string functionName, string packageName) {
            var rdData = await _functionRdDataProvider.GetFunctionRdDataAsync(functionName, packageName);
            UpdateIndex(functionName, rdData);

            IFunctionInfo fi;
            _functionToInfoMap.TryGetValue(functionName, out fi);
            return fi;
        }

        private void UpdateIndex(string functionName, string rdData) {
            IReadOnlyList<IFunctionInfo> functionInfos = GetFunctionInfosFromRd(rdData);
            foreach (IFunctionInfo info in functionInfos) {
                _functionToInfoMap[info.Name] = info;
            }
            if (!_functionToInfoMap.ContainsKey(functionName)) {
                if (functionInfos.Count > 0) {
                    // RD doesn't contain the requested function.
                    // e.g. as.Date.character has RD for as.Date but not itself
                    // without its own named info, this will request indefinitely many times
                    // as workaround, add the first info with functionName
                    _functionToInfoMap[functionName] = functionInfos[0];
                } else {
                    // Add stub function info here to prevent subsequent calls
                    // for the same function as we already know the call will fail.
                    _functionToInfoMap[functionName] = new FunctionInfo(functionName);
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

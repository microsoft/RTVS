// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.R.Support.RD.Parser;

namespace Microsoft.R.Support.Help.Functions {
    /// <summary>
    /// Provides information on functions in packages for intellisense.
    /// </summary>
    public sealed partial class FunctionIndex : IFunctionIndex {
        /// <summary>
        /// Maps package name to a list of functions in the package.
        /// Used to extract function names and descriptions when
        /// showing list of functions available in the file.
        /// </summary>
        private readonly ConcurrentDictionary<string, BlockingCollection<INamedItemInfo>> _packageToFunctionsMap = new ConcurrentDictionary<string, BlockingCollection<INamedItemInfo>>();

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

        public void Dispose() {
            _functionRdDataProvider?.Dispose();
        }

        /// <summary>
        /// Given function name provides name of the containing package
        /// </summary>
        public string GetFunctionPackage(string functionName) {
            if (_functionToPackageMap != null) {
                string packageName;
                if (_functionToPackageMap.TryGetValue(functionName, out packageName)) {
                    return packageName;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Retrieves list of functions in a given package
        /// </summary>
        public IReadOnlyCollection<INamedItemInfo> GetPackageFunctions(string packageName) {
            if (_packageToFunctionsMap != null) {
                BlockingCollection<INamedItemInfo> packageFunctions;
                if (_packageToFunctionsMap.TryGetValue(packageName, out packageFunctions)) {
                    return packageFunctions;
                }
            }
            return new List<INamedItemInfo>();
        }

        /// <summary>
        /// Retrieves function information by name. If informaton is not
        /// available, starts asynchronous retrieval of the function info
        /// from R and when the data becomes available invokes specified
        /// callback passing the parameter. This is used for async
        /// intellisense or function signature/parameter help.
        /// </summary>
        public IFunctionInfo GetFunctionInfo(string functionName, Action<object> infoReadyCallback = null, object parameter = null) {
            if (_functionToInfoMap != null) {
                IFunctionInfo functionInfo;
                if (_functionToInfoMap.TryGetValue(functionName, out functionInfo)) {
                    return functionInfo;
                } else {
                    string packageName;
                    if (_functionToPackageMap.TryGetValue(functionName, out packageName)) {
                        GetFunctionInfoFromEngineAsync(functionName, packageName, infoReadyCallback, parameter);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Fetches help on the function from R asynchronously.
        /// When function data is obtained, parsed and the function
        /// index is updated, method invokes <see cref="infoReadyCallback"/>
        /// callback passing the specified parameter. Callback method can now
        /// fetch function information from the index.
        /// </summary>
        private void GetFunctionInfoFromEngineAsync(string functionName, string packageName, Action<object> infoReadyCallback = null, object parameter = null) {
            _functionRdDataProvider.GetFunctionRdData(
                functionName,
                packageName,
                rdData => {
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
                            // TODO: add some stub function info here to prevent subsequent calls for the same function as we already know the call will fail.
                        }
                    }

                    if (infoReadyCallback != null) {
                        _shell.DispatchOnUIThread(() => {
                            infoReadyCallback(parameter);
                        });
                    }
                });
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

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;

namespace Microsoft.R.Editor.Functions {
    public static class FunctionIndexExtensions {
        /// <summary>
        /// Given function name and package name attempts to locate function information.
        /// Intended to be used from non-async capable code that works via callbacks.
        /// </summary>
        /// <param name="functionIndex">Function index</param>
        /// <param name="functionName">Function name</param>
        /// <param name="packageName">Package name (can be null if not known)</param>
        /// <param name="callback">Callback to invoke when information becomes available</param>
        /// <param name="parameter">User data to pass to the callback method</param>
        public static void GetFunctionInfoAsync(this IFunctionIndex functionIndex, string functionName, string packageName, Action<IFunctionInfo, object> callback, object parameter = null, bool mainThreadRequired = true) {
            var fi = functionIndex.GetFunctionInfo(functionName, packageName);
            if (fi != null) {
                callback(fi, parameter);
            } else {
                GetFunctionInfoFromPackageAsync(functionIndex, functionName, packageName, callback, parameter, mainThreadRequired).DoNotWait();
            }
        }

        private static async Task GetFunctionInfoFromPackageAsync(IFunctionIndex functionIndex, string functionName, string packageName, Action<IFunctionInfo, object> callback, object parameter, bool mainThreadRequired) {
            IFunctionInfo fi = null;
            packageName = packageName ?? await functionIndex.GetPackageNameAsync(functionName);
            if (!string.IsNullOrEmpty(packageName)) {
                fi = await functionIndex.GetFunctionInfoAsync(functionName, packageName);
            }
            if (mainThreadRequired) {
                await functionIndex.Services.MainThread().SwitchToAsync();
            }
            callback(fi, parameter);
        }

        public static async Task<IFunctionInfo> GetFunctionInfoAsync(this IFunctionIndex functionIndex, string functionName, string packageName = null) {
            var fi = functionIndex.GetFunctionInfo(functionName, packageName);
            if (fi != null) {
                return fi;
            }

            packageName = await functionIndex.GetPackageNameAsync(functionName);
            if (!string.IsNullOrEmpty(packageName)) {
                return functionIndex.GetFunctionInfo(functionName, packageName);
            }

            return null;
        }
    }
}

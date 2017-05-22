// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Threading;

namespace Microsoft.R.Editor.Functions {
    public static class FunctionIndexExtensions {
        public static void GetFunctionInfoAsync(this IFunctionIndex functionIndex, string functionName, string packageName, Action<IFunctionInfo, object> callback, object parameter = null) {
            var fi = functionIndex.GetFunctionInfo(functionName, packageName);
            if (fi != null) {
                callback(fi, parameter);
            } else {
                GetFunctionInfoFromPackageAsync(functionIndex, functionName, packageName, callback, parameter).DoNotWait();
            }
        }

        private static async Task GetFunctionInfoFromPackageAsync(IFunctionIndex functionIndex, string functionName, string packageName, Action<IFunctionInfo, object> callback, object parameter) {
            IFunctionInfo fi = null;
            packageName = packageName ?? await functionIndex.GetPackageNameAsync(functionName);
            if (!string.IsNullOrEmpty(packageName)) {
                fi = functionIndex.GetFunctionInfo(functionName, packageName);
            }
            await functionIndex.Services.MainThread().SwitchToAsync();
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

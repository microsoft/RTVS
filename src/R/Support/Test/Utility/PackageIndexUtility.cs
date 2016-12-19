// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Support.Help;
using Microsoft.UnitTests.Core.Mef;

namespace Microsoft.R.Support.Test.Utility {
    [ExcludeFromCodeCoverage]
    public static class PackageIndexUtility {
        public static Task<IFunctionInfo> GetFunctionInfoAsync(IFunctionIndex functionIndex, string functionName) {
            var tcs = new TaskCompletionSource<IFunctionInfo>();
            var result = functionIndex.GetFunctionInfo(functionName, null, (o, p) => {
                var r = functionIndex.GetFunctionInfo(functionName, p);
                tcs.TrySetResult(r);
            });

            if (result != null) {
                tcs.TrySetResult(result);
            }

            return tcs.Task;
        }

        public static async Task DisposeAsync(this IPackageIndex packageIndex, IExportProvider exportProvider) {
            var sessionProvider = exportProvider.GetExportedValue<IRInteractiveWorkflowProvider>().GetOrCreate().RSessions;
            if (sessionProvider != null) {
                await sessionProvider.RemoveBrokerAsync();
            }
            packageIndex?.Dispose();
        } 
    }
}
 
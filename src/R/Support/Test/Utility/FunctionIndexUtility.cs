// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Help;
using Microsoft.R.Support.Settings;
using Microsoft.UnitTests.Core.Mef;

namespace Microsoft.R.Support.Test.Utility {
    [ExcludeFromCodeCoverage]
    public static class FunctionIndexUtility {
        public static Task<IFunctionInfo> GetFunctionInfoAsync(IFunctionIndex functionIndex, string functionName) {
            IntelliSenseRHost.HostStartTimeout = 10000;

            var tcs = new TaskCompletionSource<IFunctionInfo>();
            var result = functionIndex.GetFunctionInfo(functionName, o => {
                var r = functionIndex.GetFunctionInfo(functionName);
                tcs.TrySetResult(r);
            });

            if (result != null) {
                tcs.TrySetResult(result);
            }

            return tcs.Task;
        }

        public static Task InitializeAsync(IFunctionIndex functionIndex) {
            RToolsSettings.Current = new TestRToolsSettings();
            return functionIndex.BuildIndexAsync();
        } 

        public static async Task DisposeAsync(IFunctionIndex functionIndex, IExportProvider exportProvider) {
            IRSessionProvider sessionProvider = exportProvider.GetExportedValue<IRSessionProvider>();
            if (sessionProvider != null) {
                await Task.WhenAll(sessionProvider.GetSessions().Select(s => s.StopHostAsync()));
            }
        } 
    }
}
 
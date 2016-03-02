// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Signatures;
using Microsoft.R.Support.Help.Definitions;
using Microsoft.R.Support.Help.Functions;
using Microsoft.R.Support.Settings;

namespace Microsoft.R.Support.Test.Utility {
    [ExcludeFromCodeCoverage]
    public static class FunctionIndexUtility {
        public static Task<IFunctionInfo> GetFunctionInfoAsync(string functionName) {
            FunctionRdDataProvider.HostStartTimeout = 10000;

            var tcs = new TaskCompletionSource<IFunctionInfo>();
            var result = FunctionIndex.GetFunctionInfo(functionName, o => {
                var r = FunctionIndex.GetFunctionInfo(functionName);
                tcs.TrySetResult(r);
            });

            if (result != null) {
                tcs.TrySetResult(result);
            }

            return tcs.Task;
        }

        public static Task InitializeAsync() {
            RToolsSettings.Current = new TestRToolsSettings();
            FunctionIndex.Initialize();
            return FunctionIndex.BuildIndexAsync();
        } 

        public static async Task DisposeAsync() {
            IRSessionProvider sessionProvider = EditorShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            if (sessionProvider != null) {
                await Task.WhenAll(sessionProvider.GetSessions().Select(s => s.StopHostAsync()));
            }

            FunctionIndex.Terminate();
        } 
    }
}
 
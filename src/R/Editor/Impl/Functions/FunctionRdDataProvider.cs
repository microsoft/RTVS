// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Host.Client;

namespace Microsoft.R.Editor.Functions {
    /// <summary>
    /// Provides RD data (help) on a function from the specified package.
    /// </summary>
    public sealed class FunctionRdDataProvider : IFunctionRdDataProvider {
        private readonly IIntellisenseRSession _host;

        public FunctionRdDataProvider(IServiceContainer services) {
            _host = services.GetService<IIntellisenseRSession>();
        }

        /// <summary>
        /// Asynchronously fetches RD data on the function from R.
        /// When RD data is available, invokes specified callback
        /// passing function name and the RD data extracted from R.
        /// </summary>
        public void GetFunctionRdDataAsync(string functionName, string packageName, Action<string> rdDataAvailableCallback) {
            Task.Run(async () => {
                var rd = await GetFunctionRdDataAsync(functionName, packageName);
                rdDataAvailableCallback(rd);
            });
        }

        /// <summary>
        /// Asynchronously fetches RD data on the function from R.
        /// </summary>
        public async Task<string> GetFunctionRdDataAsync(string functionName, string packageName) {
            if(packageName.EqualsOrdinal("rtvs")) {
                return GetRtvsFunctionRdData(functionName);
            }

            await TaskUtilities.SwitchToBackgroundThread();
            await _host.StartSessionAsync();

            var command = GetCommandText(functionName, packageName);
            try {
                return await _host.Session.EvaluateAsync<string>(command, REvaluationKind.Normal);
            } catch (RException) { }

            // Sometimes there is no information in a specific package.
            // For example, Matrix exports 'as.matrix' and base does it as well.
            // However, help('as.matrix', 'Matrix') will fail while help('as.matrix' will succeed.
            command = GetCommandText(functionName, null);
            try {
                return await _host.Session.EvaluateAsync<string>(command, REvaluationKind.Normal);
            } catch (RException) { }

            return string.Empty;
        }

        private string GetCommandText(string functionName, string packageName) {
            if (string.IsNullOrEmpty(packageName)) {
                return "rtvs:::signature.help1('" + functionName + "')";
            }
            return "rtvs:::signature.help2('" + functionName + "', '" + packageName + "')";
        }

        private string GetRtvsFunctionRdData(string functionName) {
            var fs = _host.Services.GetService<IFileSystem>();
            var app = _host.Services.GetService<IPlatformServices>();
            var dataFilePath = Path.Combine(app.ApplicationFolder, @"rtvs\man", Path.ChangeExtension(functionName, "rd"));
            return fs.FileExists(dataFilePath) ? fs.ReadAllText(dataFilePath) : string.Empty;
        }
    }
}

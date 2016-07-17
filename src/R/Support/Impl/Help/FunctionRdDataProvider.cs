// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.R.Support.Help;

namespace Microsoft.R.Host.Client.Signatures {
    /// <summary>
    /// Provides RD data (help) on a function from the specified package.
    /// </summary>
    [Export(typeof(IFunctionRdDataProvider))]
    public sealed class FunctionRdDataProvider : IFunctionRdDataProvider {
        private readonly IIntellisenseRHost _host;

        [ImportingConstructor]
        public FunctionRdDataProvider(IIntellisenseRHost host) {
            _host = host;
        }

        /// <summary>
        /// Asynchronously fetches RD data on the function from R.
        /// When RD data is available, invokes specified callback
        /// passing function name and the RD data extracted from R.
        /// </summary>
        public void GetFunctionRdData(string functionName, string packageName, Action<string> rdDataAvailableCallback) {
            Task.Run(async () => {
                await _host.CreateSessionAsync();
                string command = GetCommandText(functionName, packageName);
                string rd;
                try {
                    rd = await _host.Session.EvaluateAsync<string>(command, REvaluationKind.Normal);
                    rdDataAvailableCallback(rd);
                } catch (RException) { }
            });
        }

        private string GetCommandText(string functionName, string packageName) {
            if (string.IsNullOrEmpty(packageName)) {
                return "rtvs:::signature.help1('" + functionName + "')";
            }
            return "rtvs:::signature.help2('" + functionName + "', '" + packageName + "')";
        }
    }
}

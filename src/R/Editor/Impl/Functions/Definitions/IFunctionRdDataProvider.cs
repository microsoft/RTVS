// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.R.Editor.Functions {
    /// <summary>
    /// Exported via MEF. Provides RD data (help) on a
    /// function from the specified package.
    /// </summary>
    public interface IFunctionRdDataProvider {
        /// <summary>
        /// Asynchronously fetches RD data on the function from R.
        /// When RD data is available, invokes specified callback
        /// passing function name and the RD data extracted from R.
        /// </summary>
        void GetFunctionRdDataAsync(string functionName, string packageName, Action<string> rdDataAvailableCallback);
        
        /// <summary>
        /// Asynchronously fetches RD data on the function from R.
        /// </summary>
        Task<string> GetFunctionRdDataAsync(string functionName, string packageName);
    }
}

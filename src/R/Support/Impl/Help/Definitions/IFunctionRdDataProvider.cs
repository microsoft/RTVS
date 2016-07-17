// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Support.Help {
    /// <summary>
    /// Exported via MEF. Provides RD data (help) on a
    /// function from the specified package.
    /// </summary>
    public interface IFunctionRdDataProvider {
        /// <summary>
        /// Asynchronously fetches RD data on the function from R.
        /// When RD data is available, invokes specified callback
        /// passing funation name and the RD data extracted from R.
        /// </summary>
        void GetFunctionRdData(string functionName, string packageName, Action<string> rdDataAvailableCallback);
    }
}

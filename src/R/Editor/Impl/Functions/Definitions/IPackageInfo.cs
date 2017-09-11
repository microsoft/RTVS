// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.R.Editor.Functions {
    public interface IPackageInfo : INamedItemInfo {
        /// <summary>
        /// List of functions in the package
        /// </summary>
        IEnumerable<IFunctionInfo> Functions { get; }

        /// <summary>
        /// Writes information to disk for faster retrieval when new session starts
        /// </summary>
        void WriteToDisk();
    }
}

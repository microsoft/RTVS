// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Editor.Functions {
    /// <summary>
    /// Represents information about a function stored in a cache 
    /// file that contains information about functions in a package.
    /// </summary>
    public interface IPersistentFunctionInfo : INamedItemInfo {
        /// <summary>
        /// Indicates that function is internal: it has 'internal' 
        /// in its list of keywords or is not exported from the package.
        /// </summary>
        bool IsInternal { get; }
    }
}

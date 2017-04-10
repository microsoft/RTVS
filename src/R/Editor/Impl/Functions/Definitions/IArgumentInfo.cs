// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Editor.Functions {
    public interface IArgumentInfo : INamedItemInfo {
        /// <summary>
        /// Default argument value
        /// </summary>
        string DefaultValue { get; }

        /// <summary>
        /// True if argument can be omitted
        /// </summary>
        bool IsOptional { get; }

        /// <summary>
        /// Trie if argument is the '...' argument
        /// </summary>
        bool IsEllipsis { get; }
    }
}

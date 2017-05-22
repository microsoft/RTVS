// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Editor.Functions {
    /// <summary>
    /// Represents generic item that has name and description.
    /// Primarily used in intellisense where name appear in the
    /// completion list and description is shows as a tooltip.
    /// </summary>
    public interface INamedItemInfo {
        /// <summary>
        /// Type of the item (function, constant)
        /// </summary>
        NamedItemType ItemType { get; }

        /// <summary>
        /// Item name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Item description
        /// </summary>
        string Description { get; }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Editor.Functions {
    public class NamedItemInfo : INamedItemInfo {
        /// <summary>
        /// Item type: function, constant, package, ...
        /// </summary>
        public NamedItemType ItemType { get; internal set; }

        /// <summary>
        /// Item name
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Item description
        /// </summary>
        public virtual string Description { get; internal set; }

        public NamedItemInfo(string name, NamedItemType type) :
            this(name, string.Empty, type) {
        }

        public NamedItemInfo(string name, string description, NamedItemType type) {
            Name = name;
            Description = description;
            ItemType = type;
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Microsoft.R.Editor.Functions {
    public sealed class ArgumentInfo : NamedItemInfo, IArgumentInfo {
        /// <summary>
        /// Default argument value
        /// </summary>
        public string DefaultValue { get; internal set; }

        /// <summary>
        /// True if argument can be omitted
        /// </summary>
        public bool IsOptional { get; internal set; }

        /// <summary>
        /// True if argument is '...'
        /// </summary>
        public bool IsEllipsis { get; internal set; }

        public ArgumentInfo(string name) :
            this(name, string.Empty) {
        }

        public ArgumentInfo(string name, string description) :
            this(name, description, null) {
        }

        public ArgumentInfo(string name, string description, string defaultValue) :
            base(name, description, NamedItemType.Parameter) {
            DefaultValue = defaultValue;
        }
    }
}

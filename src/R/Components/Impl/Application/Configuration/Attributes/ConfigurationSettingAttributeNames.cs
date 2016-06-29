// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using static System.FormattableString;

namespace Microsoft.R.Components.Application.Configuration {
    public static class ConfigurationSettingAttributeNames {
        public const string Category = "Category";
        public const string Description = "Description";
        public const string Editor = "Editor";

        public static IReadOnlyCollection<string> KnownAttributes { get; } = new string[] { Category, Description, Editor };

        /// <summary>
        /// Returns string to use as attribute key when writing attribute
        /// into the setting file. Typically looks like [Attribute]
        /// </summary>
        public static string GetPersistentKey(string attribute) {
            return Invariant($"[{attribute}]");
        }
    }
}

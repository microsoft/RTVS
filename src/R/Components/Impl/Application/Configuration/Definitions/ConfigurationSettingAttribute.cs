// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.R.Components.Application.Configuration {
    public static class ConfigurationSettingAttribute {
        public const string Category = "[Category]";
        public const string Description = "[Description]";
        public const string Editor = "[Editor]";

        public static IReadOnlyCollection<string> Attributes { get; } = new string[] { Category, Description, Editor };

        public static string GetName(string attribute) {
            if(attribute.Length > 0 && attribute[0] =='[' && attribute[attribute.Length-1] == ']') {
                return attribute.Substring(1, attribute.Length - 2);
            }
            return string.Empty;
        }
    }
}

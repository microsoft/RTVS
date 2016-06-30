// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;

namespace Microsoft.R.Components.Application.Configuration {
    internal sealed class ConfigurationSettingDescriptionAttribute: ConfigurationSettingAttributeBase {
        public ConfigurationSettingDescriptionAttribute() : this(null) { }

        public ConfigurationSettingDescriptionAttribute(string value) : 
            base(ConfigurationSettingAttributeNames.Description, value) {
        }

        public override Attribute GetDotNetAttribute() {
            return new DescriptionAttribute(Value);
        }
    }
}

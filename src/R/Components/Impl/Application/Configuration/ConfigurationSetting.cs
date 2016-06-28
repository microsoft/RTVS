// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.R.Components.Application.Configuration {
    public sealed class ConfigurationSetting : IConfigurationSetting {
        public string Name { get; internal set; }
        public string Value { get; set; }
        public ConfigurationSettingValueType ValueType { get; internal set; }
        public IDictionary<string, string> Attributes { get; } = new Dictionary<string, string>();
    }
}

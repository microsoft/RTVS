// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Reflection;
using static System.FormattableString;

namespace Microsoft.R.Components.Application.Configuration {
    internal abstract class ConfigurationSettingAttributeBase : IConfigurationSettingAttribute {
        public ConfigurationSettingAttributeBase(string name, string value) {
            Name = name;
            Value = value;
        }

        public string Name { get; }
        public string Value { get; set; }

        public abstract Attribute GetDotNetAttribute();

        public static IConfigurationSettingAttribute CreateAttribute(string name, string value) {
            var attribute = Activator.CreateInstance(
                   Assembly.GetExecutingAssembly().FullName, 
                   Invariant($"Microsoft.R.Components.Application.Configuration.ConfigurationSetting{name}Attribute")) 
                   as IConfigurationSettingAttribute;
            attribute.Value = value;
            return attribute;
        }
    }
}

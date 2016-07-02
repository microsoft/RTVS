// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Reflection;
using static System.FormattableString;

namespace Microsoft.R.Components.Application.Configuration {
    internal abstract class ConfigurationSettingAttributeBase : ObservableNameValue<string>, IConfigurationSettingAttribute {
        public ConfigurationSettingAttributeBase(string name, string value) : base(name, value) { }

        public abstract Attribute GetDotNetAttribute();

        public static IConfigurationSettingAttribute CreateAttribute(string name, string value) {
            IConfigurationSettingAttribute attribute = null;
            try {
                var handle = Activator.CreateInstance(
                       Assembly.GetExecutingAssembly().FullName,
                       Invariant($"Microsoft.R.Components.Application.Configuration.ConfigurationSetting{name}Attribute"));
                attribute = handle.Unwrap() as IConfigurationSettingAttribute;
                attribute.Value = value;
            } catch (Exception) { }
            return attribute;
        }
    }
}

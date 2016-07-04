// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.R.Components.Application.Configuration {
    /// <summary>
    /// Base class for attributes of a configuration setting.
    /// </summary>
    internal abstract class ConfigurationSettingAttributeBase : 
        ObservableNameValue<string>, IConfigurationSettingAttribute {
        public ConfigurationSettingAttributeBase(string name, string value) : 
            base(name, value) { }

        /// <summary>
        /// Returns the respective .NET attribute
        /// </summary>
        /// <returns></returns>
        public abstract Attribute GetDotNetAttribute();
    }
}

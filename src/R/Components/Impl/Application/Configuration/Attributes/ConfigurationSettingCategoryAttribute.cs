// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.R.Components.Application.Configuration {
    internal sealed class ConfigurationSettingCategoryAttribute : ConfigurationSettingAttributeBase {
        public ConfigurationSettingCategoryAttribute() : this(null) { }

        public ConfigurationSettingCategoryAttribute(string value) :
            base(ConfigurationSettingAttributeNames.Category, value) {
        }

        public override Attribute GetDotNetAttribute() {
            return new CategoryAttribute(Value);
        }

        [Export(typeof(IConfigurationSettingAttributeFactory))]
        [Name(ConfigurationSettingAttributeNames.Category)]
        internal sealed class AttributeFactory : IConfigurationSettingAttributeFactory {
            public IConfigurationSettingAttribute CreateInstance(string value) {
                return new ConfigurationSettingCategoryAttribute(value);
            }
        }
    }
}

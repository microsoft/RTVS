// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.R.Common.Wpf.Controls;

namespace Microsoft.R.Components.Application.Configuration {
    public sealed class ConfigurationSetting : BindableBase, IConfigurationSetting {
        private string _value;
        private ConfigurationSettingValueType _valueType;
        private string _category;
        private string _description;
        private string _editorType;

        public string Name { get; internal set; }

        public string Value {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public ConfigurationSettingValueType ValueType {
            get => _valueType;
            set => SetProperty(ref _valueType, value);
        }

        public string Category {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public string Description {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string EditorType {
            get => _editorType;
            set => SetProperty(ref _editorType, value);
        }

        public ConfigurationSetting(): this(null) { }
        public ConfigurationSetting(string name) : this(name, null, ConfigurationSettingValueType.String) { }

        public ConfigurationSetting(string name, string value, ConfigurationSettingValueType valueType) {
            Name = name;
            Value = value;
            ValueType = valueType;
        }
    }
}

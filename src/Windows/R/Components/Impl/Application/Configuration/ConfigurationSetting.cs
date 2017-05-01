// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Common.Wpf;

namespace Microsoft.R.Components.Application.Configuration {
    public sealed class ConfigurationSetting : BindableBase, IConfigurationSetting {
        private string _value;
        private ConfigurationSettingValueType _valueType;
        private string _category;
        private string _description;
        private string _editorType;

        public string Name { get; internal set; }

        public string Value {
            get { return _value; }
            set { SetProperty(ref _value, value, "Value"); }
        }

        public ConfigurationSettingValueType ValueType {
            get { return _valueType; }
            set { SetProperty(ref _valueType, value, "ValueType"); }
        }

        public string Category {
            get { return _category; }
            set { SetProperty(ref _category, value, "Category"); }
        }

        public string Description {
            get { return _description; }
            set { SetProperty(ref _description, value, "Description"); }
        }

        public string EditorType {
            get { return _editorType; }
            set { SetProperty(ref _editorType, value, "EditorType"); }
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

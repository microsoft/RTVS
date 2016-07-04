// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Microsoft.R.Components.Application.Configuration {
    public sealed class ConfigurationSetting : IConfigurationSetting {
        private string _value;
        private ConfigurationSettingValueType _valueType;
        private string _category;
        private string _description;
        private string _editorType;

        public string Name { get; internal set; }

        public string Value {
            get { return _value; }
            set {
                if(_value != value) {
                    _value = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Value"));
                }
            }
        }

        public ConfigurationSettingValueType ValueType {
            get { return _valueType; }
            set {
                if (_valueType != value) {
                    _valueType = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ValueType"));
                }
            }
        }
        public string Category {
            get { return _category; }
            set {
                if (_category != value) {
                    _category = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Category"));
                }
            }
        }

        public string Description {
            get { return _description; }
            set {
                if (_description != value) {
                    _description = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Description"));
                }
            }
        }

        public string EditorType {
            get { return _editorType; }
            set {
                if (_editorType != value) {
                    _editorType = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("EditorType"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ConfigurationSetting(): this(null) { }
        public ConfigurationSetting(string name) : this(name, null, ConfigurationSettingValueType.String) { }

        public ConfigurationSetting(string name, string value, ConfigurationSettingValueType valueType) {
            Name = name;
            Value = value;
        }
    }
}

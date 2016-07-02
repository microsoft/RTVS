// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Microsoft.R.Components.Application.Configuration {
    public sealed class ConfigurationSetting : IConfigurationSetting {
        private readonly ObservableCollection<IConfigurationSettingAttribute> _attributes = new ObservableCollection<IConfigurationSettingAttribute>();
        private string _value;
        private ConfigurationSettingValueType _valueType;

        public string Name { get; internal set; }

        public string Value {
            get { return _value; }
            set {
                if(_value != value) {
                    _value = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Name));
                }
            }
        }

        public ConfigurationSettingValueType ValueType {
            get { return _valueType; }
            set {
                if (_valueType != value) {
                    _valueType = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(Name));
                }
            }
        }
        public ObservableCollection<IConfigurationSettingAttribute> Attributes => _attributes;

        public event PropertyChangedEventHandler PropertyChanged;

        public ConfigurationSetting(): this(null) { }
        public ConfigurationSetting(string name) : this(name, null, ConfigurationSettingValueType.String) { }

        public ConfigurationSetting(string name, string value, ConfigurationSettingValueType valueType) {
            Name = name;
            Value = value;
            ValueType = valueType;

            _attributes.CollectionChanged += OnAttributesChanged;
        }

        private void OnAttributesChanged(object sender, NotifyCollectionChangedEventArgs e) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Attributes)));
        }
    }
}

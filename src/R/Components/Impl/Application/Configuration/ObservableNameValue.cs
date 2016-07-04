// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;

namespace Microsoft.R.Components.Application.Configuration {
    public class ObservableNameValue<T> : INotifyPropertyChanged {
        private T _value;

        public ObservableNameValue(string name, T value) {
            Name = name;
            _value = value;
        }

        public string Name { get; }
        public T Value {
            get { return _value; }
            set {
                _value = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

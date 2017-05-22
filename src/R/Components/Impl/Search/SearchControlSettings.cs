// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Microsoft.R.Components.Search {
    public class SearchControlSettings {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();
        
        public uint MinWidth {
            get => GetValue<uint>();
            set => SetValue(value);
        }

        public uint MaxWidth {
            get => GetValue<uint>();
            set => SetValue(value);
        }

        public Guid SearchCategory {
            get => GetValue<Guid>();
            set => SetValue(value);
        }

        private T GetValue<T>([CallerMemberName] string propertyName = null) => TryGetValue(propertyName, out T value) ? value : default(T);

        private void SetValue<T>(T value, [CallerMemberName] string propertyName = null) => _values[propertyName] = value;

        public bool TryGetValue<T>(string propertyName, out T value) {
            if (_values.TryGetValue(propertyName, out var objValue)) {
                value = (T)objValue;
                return true;
            }

            value = default(T);
            return false;
        }
    }
}
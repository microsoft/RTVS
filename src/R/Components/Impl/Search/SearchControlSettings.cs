// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Microsoft.R.Components.Search {
    public class SearchControlSettings {
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();
        
        public uint MinWidth {
            get { return GetValue<uint>(); }
            set { SetValue(value); }
        }

        public uint MaxWidth {
            get { return GetValue<uint>(); }
            set { SetValue(value); }
        }

        public Guid SearchCategory {
            get { return GetValue<Guid>(); }
            set { SetValue(value); }
        }

        private T GetValue<T>([CallerMemberName] string propertyName = null) {
            object value;
            return _values.TryGetValue(propertyName, out value) ? (T) value : default(T);
        }

        private void SetValue<T>(T value, [CallerMemberName] string propertyName = null) {
            _values[propertyName] = value;
        }

        public bool TryGetValue(string propertyName, out object value) {
            return _values.TryGetValue(propertyName, out value);
        }
    }
}
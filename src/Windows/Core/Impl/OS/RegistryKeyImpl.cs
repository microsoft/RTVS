// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Win32;

namespace Microsoft.Common.Core.OS {
    internal sealed class RegistryKeyImpl : IRegistryKey {
        RegistryKey _key;

        public RegistryKeyImpl(RegistryKey key) {
            _key = key;
        }

        public void Dispose() {
            _key?.Dispose();
            _key = null;
        }

        public string[] GetSubKeyNames() => _key?.GetSubKeyNames() ?? new string[0];
        public object GetValue(string name) => _key?.GetValue(name);
        public void SetValue(string name, object value) => _key?.SetValue(name, value);

        public IRegistryKey OpenSubKey(string name, bool writable = false) {
            var key = _key.OpenSubKey(name, writable);
            if(key == null && writable) {
                key = _key.CreateSubKey(name, true);
            }
            return new RegistryKeyImpl(key);
        }
    }
}

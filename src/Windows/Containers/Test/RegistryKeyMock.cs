// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core.OS;
using Microsoft.Win32;

namespace Microsoft.R.Containers.Windows.Test {
    class RegistryKeyMock : IRegistryKey {
        RegistryKey _key;
        IEnumerable<string> _failPaths;
        public RegistryKeyMock(RegistryKey key, IEnumerable<string> failPaths) {
            _key = key;
            _failPaths = failPaths;
        }

        public void Dispose() {
            _key?.Dispose();
            _key = null;
        }

        public string[] GetSubKeyNames() => _key?.GetSubKeyNames() ?? new string[0];
        public string[] GetValueNames() => _key.GetValueNames() ?? new string[0];
        public object GetValue(string name) => _key?.GetValue(name);
        public void SetValue(string name, object value) => _key?.SetValue(name, value);

        public IRegistryKey OpenSubKey(string name, bool writable = false) {
            if (_failPaths.Contains(name)) {
                return null;
            }

            var key = _key.OpenSubKey(name, writable);
            if (key == null && writable) {
                key = _key.CreateSubKey(name, true);
            }
            return new RegistryKeyMock(key, _failPaths);
        }
    }
}

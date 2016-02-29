// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Common.Core.OS;

namespace Microsoft.R.Actions.Test.Installation {
    internal sealed class RegistryKeyMock : IRegistryKey {
        private readonly RegistryKeyMock[] _subkeys;
        private Dictionary<string, string> _values = new Dictionary<string, string>();

        public string Name { get; }

        public RegistryKeyMock(string name) : this(name, new RegistryKeyMock[0]) { }
        public RegistryKeyMock(string name, params RegistryKeyMock[] subkeys) : this(name, subkeys, new string[0], new string[0]) { }
        public RegistryKeyMock(string name, RegistryKeyMock[] subkeys, string[] valueNames, string[] values) {
            Name = name;
            _subkeys = subkeys;
            for (int i = 0; i < valueNames.Length; i++) {
                _values[valueNames[i]] = values[i];
            }
        }

        public void Dispose() {
        }

        public string[] GetSubKeyNames() {
            return _subkeys.Select(x => x.Name).ToArray();
        }

        public object GetValue(string name) {
            return _values[name];
        }

        public IRegistryKey OpenSubKey(string name) {
            return _subkeys.FirstOrDefault(x => x.Name == name);
        }
    }
}

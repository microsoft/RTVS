// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Common.Core.OS;

namespace Microsoft.Common.Core.Test.Registry {
    [ExcludeFromCodeCoverage]
    public sealed class RegistryKeyMock : IRegistryKey {
        private readonly RegistryKeyMock[] _subkeys;
        private Dictionary<string, string> _values = new Dictionary<string, string>();

        public string Name { get; }

        public RegistryKeyMock(string name) : this(name, new RegistryKeyMock[0]) { }
        public RegistryKeyMock(string name, params RegistryKeyMock[] subkeys) : this(name, subkeys, new string[0], new string[0]) { }
        public RegistryKeyMock(string name, RegistryKeyMock[] subkeys = null, string[] valueNames = null, string[] values = null) {
            Name = name;
            _subkeys = subkeys ?? new RegistryKeyMock[0];
            for (int i = 0; valueNames != null && i < valueNames.Length; i++) {
                _values[valueNames[i]] = values[i];
            }
        }

        public void Dispose() {
        }

        public string[] GetSubKeyNames() {
            return _subkeys.Select(x => x.Name).ToArray();
        }

        public object GetValue(string name) {
            return _values.ContainsKey(name) ? _values[name] : null;
        }

        public void SetValue(string name, object value) {
            _values[name] = value.ToString();
        }

        public IRegistryKey OpenSubKey(string name, bool writable = false) {
            return _subkeys.FirstOrDefault(x => x.Name == name);
        }
    }
}

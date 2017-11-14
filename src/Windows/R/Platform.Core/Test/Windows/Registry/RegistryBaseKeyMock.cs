// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.R.Platform.OS;

namespace Microsoft.Common.Core.Test.Registry {
    [ExcludeFromCodeCoverage]
    public sealed class RegistryBaseKeyMock : IRegistryKey {
        private readonly RegistryKeyMock[] _subkeys;
        private readonly  Dictionary<string, object> _values = new Dictionary<string, object>();

        public RegistryBaseKeyMock(RegistryKeyMock[] subkeys) {
            _subkeys = subkeys;
        }

        public void Dispose() {
        }

        public string[] GetSubKeyNames() => _subkeys.Select(x => x.Name).ToArray();
        public object GetValue(string name) => _values.ContainsKey(name) ? _values[name] : null;
        public void SetValue(string name, object value) => _values[name] = value;
        public IRegistryKey OpenSubKey(string name, bool writable = false) => _subkeys.FirstOrDefault(x => x.Name == name);
        public string[] GetValueNames() => _values.Keys.ToArray();
    }
}

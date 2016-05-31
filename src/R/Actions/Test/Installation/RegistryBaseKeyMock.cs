// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using Microsoft.Common.Core.OS;

namespace Microsoft.R.Actions.Test.Installation {
    internal sealed class RegistryBaseKeyMock : IRegistryKey {
        RegistryKeyMock[] _subkeys;
        public RegistryBaseKeyMock(RegistryKeyMock[] subkeys) {
            _subkeys = subkeys;
        }

        public void Dispose() {
        }

        public string[] GetSubKeyNames() {
            return _subkeys.Select(x => x.Name).ToArray();
        }

        public object GetValue(string name) {
            throw new NotImplementedException();
        }

        public void SetValue(string name, object value) {
            throw new NotImplementedException();
        }

        public IRegistryKey OpenSubKey(string name, bool writable = false) {
            return _subkeys.FirstOrDefault(x => x.Name == name);
        }
    }
}

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

        public IRegistryKey OpenSubKey(string name) {
            return _subkeys.FirstOrDefault(x => x.Name == name);
        }
    }
}

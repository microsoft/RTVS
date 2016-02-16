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

        public string[] GetSubKeyNames() {
            return _key != null ? _key.GetSubKeyNames() : new string[0];
        }

        public object GetValue(string name) {
            return _key != null ? _key.GetValue(name) : null;
        }

        public IRegistryKey OpenSubKey(string name) {
            return new RegistryKeyImpl(_key.OpenSubKey(name));
        }
    }
}

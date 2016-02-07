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
            return _key.GetSubKeyNames();
        }

        public object GetValue(string name) {
            return _key.GetValue(name);
        }

        public IRegistryKey OpenSubKey(string name) {
            return new RegistryKeyImpl(_key.OpenSubKey(name));
        }
    }
}

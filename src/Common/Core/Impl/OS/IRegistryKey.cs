using System;

namespace Microsoft.Common.Core.OS {
    public interface IRegistryKey : IDisposable {
        object GetValue(string name);
        string[] GetSubKeyNames();
        IRegistryKey OpenSubKey(string name);
    }
}

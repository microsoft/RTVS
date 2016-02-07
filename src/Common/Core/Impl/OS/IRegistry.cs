
namespace Microsoft.Common.Core.OS {
    public interface IRegistry {
        IRegistryKey OpenBaseKey(Win32.RegistryHive hive, Win32.RegistryView view);
    }
}

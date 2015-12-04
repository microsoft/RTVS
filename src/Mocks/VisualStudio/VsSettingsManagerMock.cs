using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.Shell.Mocks {
    [ExcludeFromCodeCoverage]
    public sealed class VsSettingsManagerMock : IVsSettingsManager {
        public int GetApplicationDataFolder(uint folder, out string folderPath) {
            folderPath = Path.GetTempPath();
            return VSConstants.S_OK;
        }

        public int GetCollectionScopes(string collectionPath, out uint scopes) {
            throw new NotImplementedException();
        }

        public int GetCommonExtensionsSearchPaths(uint paths, string[] commonExtensionsPaths, out uint actualPaths) {
            throw new NotImplementedException();
        }

        public int GetPropertyScopes(string collectionPath, string propertyName, out uint scopes) {
            throw new NotImplementedException();
        }

        public int GetReadOnlySettingsStore(uint scope, out IVsSettingsStore store) {
            store = new VsSettingsStoreMock();
            return VSConstants.S_OK;
        }

        public int GetWritableSettingsStore(uint scope, out IVsWritableSettingsStore writableStore) {
            writableStore = new VsSettingsStoreMock();
            return VSConstants.S_OK;
        }
    }
}

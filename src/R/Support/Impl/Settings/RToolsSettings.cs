using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Support.Settings.Definitions;

namespace Microsoft.R.Support.Settings {
    public static class RToolsSettings {

        private static IRToolsSettings _instance;
        private static ExportProvider _exportProvider;

        public static IRToolsSettings Current {
            get {
                if (_instance == null) {
                    Debug.Assert(EditorShell.Current != null);
                    if (EditorShell.Current != null) {
                        Init(EditorShell.Current.ExportProvider);
                    }
                }

                Debug.Assert(_instance != null);
                return _instance;
            }
            internal set {
                // Tests only
                _instance = value;
            }
        }

        public static void Init(ExportProvider exportProvider) {
            _exportProvider = exportProvider;
            _instance = _exportProvider != null ? _exportProvider.GetExport<IRToolsSettings>().Value : null;
            _instance?.LoadFromStorage();
        }
    }
}

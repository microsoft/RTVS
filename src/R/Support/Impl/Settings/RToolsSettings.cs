using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Support.Settings.Definitions;

namespace Microsoft.R.Support.Settings {
    public static class RToolsSettings {
        private static IRToolsSettings _settings;

        public static IRToolsSettings Current {
            get {
                if (_settings == null) {
                    _settings = EditorShell.Current.ExportProvider.GetExport<IRToolsSettings>().Value;
                }
                return _settings;
            }
            set {
                _settings = value;
            }
        }
    }
}

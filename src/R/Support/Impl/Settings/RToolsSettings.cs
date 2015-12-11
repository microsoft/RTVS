using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using Microsoft.R.Support.Settings.Definitions;

namespace Microsoft.R.Support.Settings {
    public static class RToolsSettings {
        public static IRToolsSettings Current { get; set; }

        public static void Init(ExportProvider exportProvider) {
            Current = exportProvider != null ? exportProvider.GetExport<IRToolsSettings>().Value : null;
            Debug.Assert(Current != null);
            Current?.LoadFromStorage();
        }
    }
}

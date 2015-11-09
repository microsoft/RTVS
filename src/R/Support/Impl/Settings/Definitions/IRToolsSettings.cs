using Microsoft.Common.Core.Enums;

namespace Microsoft.R.Support.Settings.Definitions {
    public interface IRToolsSettings {
        void LoadFromStorage();

        /// <summary>
        /// Path to 64-bit R installation such as 
        /// 'C:\Program Files\R\R-3.2.2' without bin\x64
        /// </summary>
        string RBasePath { get; set; }

        /// <summary>
        /// Selected CRAN mirror
        /// </summary>
        string CranMirror { get; set; }

        YesNoAsk LoadRDataOnProjectLoad { get; set; }
        YesNoAsk SaveRDataOnProjectUnload { get; set; }

        string WorkingDirectory { get; set; }
    }
}

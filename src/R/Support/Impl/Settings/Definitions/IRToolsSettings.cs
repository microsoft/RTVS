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

        bool AlwaysSaveHistory { get; set; }
        bool ClearFilterOnAddHistory { get; set; }
        bool MultilineHistorySelection { get; set; }

        /// <summary>
        /// Current working directory for REPL
        /// </summary>
        string WorkingDirectory { get; set; }

        /// <summary>
        /// Most recently used directories in REPL
        /// </summary>
        string[] WorkingDirectoryList { get; set; }

        /// <summary>
        /// Additional command line arguments to pass
        /// to the R Host process
        /// </summary>
        string RCommandLineArguments { get; set; }
    }
}

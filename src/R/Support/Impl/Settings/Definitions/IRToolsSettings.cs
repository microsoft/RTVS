using Microsoft.Common.Core.Enums;
using Microsoft.R.Components.Settings;

namespace Microsoft.R.Support.Settings.Definitions {
    public interface IRToolsSettings : IRSettings {
        YesNoAsk LoadRDataOnProjectLoad { get; set; }
        YesNoAsk SaveRDataOnProjectUnload { get; set; }

        /// <summary>
        /// Most recently used directories in REPL
        /// </summary>
        string[] WorkingDirectoryList { get; set; }

        /// <summary>
        /// Determines if R Tools should always be using external Web browser or
        /// try and send Help pages to the Help window and other Web requests 
        /// to the external default Web browser.
        /// </summary>
        HelpBrowserType HelpBrowser { get; set; }

        bool ShowDotPrefixedVariables { get; set; }
    }
}

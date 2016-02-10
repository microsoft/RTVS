using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Common.Core.Enums;
using Microsoft.R.Support.Settings.Definitions;

namespace Microsoft.R.Support.Test.Utility {
    [ExcludeFromCodeCoverage]
    [Export(typeof(IRToolsSettings))]
    public sealed class TestRToolsSettings : IRToolsSettings {
        public string CranMirror {
            get { return string.Empty; }
            set { }
        }

        public string RBasePath {
            get {
                // Test settings are fixed and are unrelated to what is stored in VS.
                // Therefore we need to look up R when it is not in the registry.
                foreach(string dir in Directory.EnumerateDirectories(@"C:\Program Files\R")) {
                    if(dir.Contains("R-3.2")) {
                        return dir;
                    }
                }
                return string.Empty;
            }
            set { }
        }

        public bool EscInterruptsCalculation {
            get { return true; }
            set { }
        }

        public YesNoAsk LoadRDataOnProjectLoad {
            get { return YesNoAsk.Yes; }
            set { }
        }

        public YesNoAsk SaveRDataOnProjectUnload {
            get { return YesNoAsk.Yes; }
            set { }
        }

        public bool AlwaysSaveHistory {
            get { return true; }
            set { }
        }

        public bool ClearFilterOnAddHistory {
            get { return true; }
            set { }
        }

        public bool MultilineHistorySelection {
            get { return true; }
            set { }
        }

        public void LoadFromStorage() {
        }

        public string WorkingDirectory { get; set; } = string.Empty;

        public string[] WorkingDirectoryList { get; set; } = new string[0];

        public string RCommandLineArguments { get; set; }

        public HelpBrowserType HelpBrowser {
            get { return HelpBrowserType.Automatic; }
            set { }
        }

        public bool ShowDotPrefixedVariables { get; set; }
    }
}

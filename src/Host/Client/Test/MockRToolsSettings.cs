using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Common.Core.Enums;
using Microsoft.R.Support.Settings.Definitions;

namespace Microsoft.R.Host.Client.Test {
    class MockRToolsSettings : IRToolsSettings {
        public bool AlwaysSaveHistory {
            get {
                return false;
            }

            set {
            }
        }

        public string CranMirror {
            get {
                return String.Empty;
            }

            set {
            }
        }

        public YesNoAsk LoadRDataOnProjectLoad {
            get {
                return YesNoAsk.No;
            }

            set {
            }
        }

        public string RBasePath {
            get {
                return String.Empty;
            }

            set {
            }
        }

        public string RCommandLineArguments {
            get {
                return String.Empty;
            }

            set {
            }
        }

        public YesNoAsk SaveRDataOnProjectUnload {
            get {
                return YesNoAsk.No;
            }

            set {
            }
        }

        public bool UseExperimentalGraphicsDevice {
            get {
                return true;
            }

            set {
            }
        }

        public string WorkingDirectory {
            get {
                return String.Empty;
            }

            set {
            }
        }

        public string[] WorkingDirectoryList {
            get {
                return new string[0];
            }

            set {
            }
        }

        public void LoadFromStorage() {
        }
    }
}

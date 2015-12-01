using System;

namespace Microsoft.R.Actions.Utility {
    public class RInstallData {
        public RInstallStatus Status { get; set; }
        public string Path { get; set; }
        public Version Version { get; set; }

        public Exception Exception { get; set; }
    }
}

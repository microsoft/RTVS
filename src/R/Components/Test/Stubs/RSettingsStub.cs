using Microsoft.R.Components.Settings;

namespace Microsoft.R.Components.Test.Stubs {
    public sealed class RSettingsStub : IRSettings {
        public bool AlwaysSaveHistory { get; set; }
        public bool ClearFilterOnAddHistory { get; set; }
        public bool MultilineHistorySelection { get; set; }
        public string RBasePath { get; set; }
        public string CranMirror { get; set; }
        public string RCommandLineArguments { get; set; }
    }
}

namespace Microsoft.R.Components.Settings {
    public interface IRSettings {
        bool AlwaysSaveHistory { get; set; }
        bool ClearFilterOnAddHistory { get; set; }
        bool MultilineHistorySelection { get; set; }
    }
}

namespace Microsoft.VisualStudio.R.Package.Telemetry.Definitions {
    internal interface ITelemetry {
        void ReportConfiguration();
        void ReportSettings();
        void ReportWindowLayout();
    }
}

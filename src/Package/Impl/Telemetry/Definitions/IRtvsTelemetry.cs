using System;

namespace Microsoft.VisualStudio.R.Package.Telemetry.Definitions {
    /// <summary>
    /// Represents telemetry operations in RTVS
    /// </summary>
    internal interface IRtvsTelemetry : IDisposable {
        void ReportConfiguration();
        void ReportSettings();
        void ReportWindowLayout();
    }
}

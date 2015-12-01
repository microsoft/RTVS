using System.Collections.Generic;

namespace Microsoft.Common.Core.Telemetry {
    /// <summary>
    /// Represents telemetry event that spans some time interval.
    /// </summary>
    public interface ITelemetryActivity {
        /// <summary>
        /// Activity properties
        /// </summary>
        IDictionary<string, object> Properties { get; }

        /// <summary>
        /// End the activity and report the event
        /// </summary>
        void RecordActivity();

        /// <summary>
        /// End the activity but will not report the event
        /// </summary>
        void CancelActivity();
    }
}

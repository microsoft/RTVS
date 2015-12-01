using System;
using System.Collections.Generic;

namespace Microsoft.Common.Core.Telemetry {
    /// <summary>
    /// Represents telemetry event that spans some time interval.
    /// Dispose activity to record inner events.
    /// </summary>
    public interface ITelemetryActivity: IDisposable {
        /// <summary>
        /// Activity properties
        /// </summary>
        IDictionary<string, object> Properties { get; }

        /// <summary>
        /// End the activity but will not report the event
        /// </summary>
        void CancelActivity();
    }
}

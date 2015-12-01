using System.Collections.Generic;

namespace Microsoft.Common.Core.Telemetry {
    /// <summary>
    /// Represents object that records telemetry events and is called by
    /// the telemetry service. In Visual Studio environment maps to IVsTelemetryService
    /// whereas in tests can be replaced by an object that writes events to a string.
    /// </summary>
    public interface ITelemetryRecorder {
        /// <summary>
        /// True if telemetry is actually being recorded
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Indicates if telemetry can collect private information
        /// </summary>
        bool CanCollectPrivateInformation { get; }

        /// <summary>
        /// Records a simple event without parameters.
        /// </summary>
        void RecordEvent(string eventName);

        /// <summary>
        /// Records event with a single parameter
        /// </summary>
        void RecordEvent(string eventName, string parameterName, object parameterValue);

        /// <summary>
        /// Records event with multiple parameters
        /// </summary>
        void RecordEvent(string eventName, IDictionary<string, object> parameters);

        /// <summary>
        /// Records telemetry activity (typically VS TelemetryActivity object)
        /// </summary>
        /// <param name="activity"></param>
        void RecordActivity(object activity);
    }
}

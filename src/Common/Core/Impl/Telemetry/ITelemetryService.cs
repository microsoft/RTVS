using System.Collections.Generic;

namespace Microsoft.Common.Core.Telemetry {
    /// <summary>
    /// Application telemetry service. In Visual Studio maps to IVsTelemetrySession.
    /// </summary>
    public interface ITelemetryService {
        /// <summary>
        /// True of user opted in and telemetry is being collected
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// Records a simple event without parameters.
        /// </summary>
        /// <param name="area">Telemetry area name such as 'Toolbox'.</param>
        /// <param name="eventName">Event name</param>
        void ReportEvent(TelemetryArea area, string eventName);

        /// <summary>
        /// Records event with a single parameter
        /// </summary>
        /// <param name="area">Telemetry area name such as 'Toolbox'.</param>
        /// <param name="eventName">Event name.</param>
        /// <param name="parameterName">Event parameter name.</param>
        /// <param name="parameterValue">Event parameter value.</param>
        void ReportEvent(TelemetryArea area, string eventName, string parameterName, object parameterValue);

        /// <summary>
        /// Records event with multiple parameters
        /// </summary>
        /// <param name="area">Telemetry area name such as 'Toolbox'.</param>
        /// <param name="eventName">Event name.</param>
        /// <param name="parameters">Event parameters.</param>
        void ReportEvent(TelemetryArea area, string eventName, IReadOnlyDictionary<string, object> parameters);

        /// <summary>
        /// Provides a way to create and start recording user activity.
        /// Activity is a parent object or scope for multiple telemetry
        /// events. For example, code refactoring or build may be recorded
        /// as an activity (i.e. as a set of related events).
        /// Dispose of the return value when the activity is complete.
        /// </summary>
        ITelemetryActivity StartActivity(TelemetryArea area, string eventName);
    }
}

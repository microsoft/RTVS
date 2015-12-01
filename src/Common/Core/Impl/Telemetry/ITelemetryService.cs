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
        /// Records event with parameters
        /// </summary>
        /// <param name="area">Telemetry area name such as 'Project'.</param>
        /// <param name="eventName">Event name.</param>
        /// <param name="parameters">
        /// Either string/object dictionary or anonymous
        /// collection of string/object pairs.
        /// </param>
        void ReportEvent(TelemetryArea area, string eventName, object parameters = null);

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

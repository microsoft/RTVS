using System.Collections.Generic;
using Microsoft.Common.Core.Diagnostics;

namespace Microsoft.Common.Core.Telemetry {
    /// <summary>
    /// Base telemetry service implementation, common to production code and test cases.
    /// </summary>
    public abstract class TelemetryServiceBase : ITelemetryService {
        public string EventNamePrefix { get; private set; }
        public string PropertyNamePrefix { get; private set; }

        /// <summary>
        /// Current active telemetry writer. Inside Visual Studio it 
        /// uses IVsTelemetryService, in unit or component tests
        /// recorder is a simple string container or a disk file.
        /// </summary>
        public ITelemetryRecorder TelemetryRecorder { get; internal set; }

        protected TelemetryServiceBase(string eventNamePrefix, string propertyNamePrefix, ITelemetryRecorder telemetryRecorder) {
            this.TelemetryRecorder = telemetryRecorder;
            this.EventNamePrefix = eventNamePrefix;
            this.PropertyNamePrefix = propertyNamePrefix;
        }

        #region ITelemetryService
        /// <summary>
        /// True of user opted in and telemetry is being collected
        /// </summary>
        public bool IsEnabled {
            get {
                return this.TelemetryRecorder.IsEnabled;
            }
        }

        public bool CanCollectPrivateInformation {
            get {
                return (this.TelemetryRecorder.IsEnabled && this.TelemetryRecorder.CanCollectPrivateInformation);
            }
        }

        /// <summary>
        /// Records a simple event without parameters.
        /// </summary>
        /// <param name="area">Telemetry area name such as 'Toolbox'.</param>
        /// <param name="eventName">Event name</param>
        public void ReportEvent(TelemetryArea area, string eventName) {
            Check.ArgumentStringNullOrEmpty("eventName", eventName);

            string fullEventName = this.EventNamePrefix + area.ToString() + "/" + eventName;
            this.TelemetryRecorder.RecordEvent(fullEventName);
        }

        /// Records event with a single parameter
        /// </summary>
        /// <param name="area">Telemetry area name such as 'Toolbox'.</param>
        /// <param name="eventName">Event name.</param>
        /// <param name="parameterName">Event parameter name.</param>
        /// <param name="parameterValue">Event parameter value.</param>
        public void ReportEvent(TelemetryArea area, string eventName, string parameterName, object parameterValue) {
            Check.ArgumentStringNullOrEmpty("eventName", eventName);
            Check.ArgumentStringNullOrEmpty("parameterName", parameterName);
            Check.ArgumentNull("parameterValue", parameterValue);

            this.TelemetryRecorder.RecordEvent(
                this.EventNamePrefix + area.ToString() + "/" + eventName,
                this.PropertyNamePrefix + area.ToString() + "." + parameterName,
                parameterValue);
        }

        /// <summary>
        /// Records event with multiple parameters
        /// </summary>
        /// <param name="area">Telemetry area name such as 'Toolbox'.</param>
        /// <param name="eventName">Event name.</param>
        /// <param name="parameters">Event parameters.</param>
        /// <summary>
        public void ReportEvent(TelemetryArea area, string eventName, IReadOnlyDictionary<string, object> parameters) {
            Check.ArgumentStringNullOrEmpty("eventName", eventName);
            Check.ArgumentNull("parameters", parameters);
            Check.ArgumentOutOfRange("parameters", () => parameters.Count <= 0);

            Dictionary<string, object> dict = new Dictionary<string, object>(parameters.Count);

            foreach (KeyValuePair<string, object> kvp in parameters) {
                Check.ArgumentStringNullOrEmpty("parameterName", kvp.Key);
                Check.ArgumentNull("parameterValue", kvp.Value);

                dict[this.PropertyNamePrefix + area.ToString() + "." + kvp.Key] = kvp.Value;
            }

            this.TelemetryRecorder.RecordEvent(this.EventNamePrefix + area.ToString() + "/" + eventName, dict);
        }

        /// <summary>
        /// Start a telemetry activity, dispose of the return value when the activity is complete
        /// </summary>
        public abstract ITelemetryActivity StartActivity(TelemetryArea area, string eventName);
        #endregion
    }
}

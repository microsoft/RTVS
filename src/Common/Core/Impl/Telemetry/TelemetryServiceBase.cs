using System;
using System.Collections.Generic;
using Microsoft.Common.Core.Diagnostics;

namespace Microsoft.Common.Core.Telemetry {
    /// <summary>
    /// Base telemetry service implementation, common to production code and test cases.
    /// </summary>
    public abstract class TelemetryServiceBase : ITelemetryService, IDisposable {
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
        /// Records event with parameters
        /// </summary>
        /// <param name="area">Telemetry area name such as 'Toolbox'.</param>
        /// <param name="eventName">Event name.</param>
        /// <param name="parameters">
        /// Either string/object dictionary or anonymous
        /// collection of string/object pairs.
        /// </param>
        /// <summary>
        public void ReportEvent(TelemetryArea area, string eventName, object parameters = null) {
            Check.ArgumentStringNullOrEmpty("eventName", eventName);

            if (parameters == null) {
                this.TelemetryRecorder.RecordEvent(this.EventNamePrefix + area.ToString() + "/" + eventName);
            } else if (parameters is string) {
                this.TelemetryRecorder.RecordEvent(this.EventNamePrefix + area.ToString() + "/" + eventName, parameters as string);
            } else {
                IDictionary<string, object> dict = DictionaryExtension.FromAnonymousObject(parameters);
                IDictionary<string, object> dictWithPrefix = new Dictionary<string, object>();

                foreach (KeyValuePair<string, object> kvp in dict) {
                    Check.ArgumentStringNullOrEmpty("parameterName", kvp.Key);
                    dictWithPrefix[this.PropertyNamePrefix + area.ToString() + "." + kvp.Key] = kvp.Value ?? string.Empty;
                }
                this.TelemetryRecorder.RecordEvent(this.EventNamePrefix + area.ToString() + "/" + eventName, dictWithPrefix);
            }
        }

        /// <summary>
        /// Start a telemetry activity, dispose of the return value when the activity is complete
        /// </summary>
        public abstract ITelemetryActivity StartActivity(TelemetryArea area, string eventName);
        #endregion

        protected virtual void Dispose(bool disposing) { }

        public void Dispose() {
            Dispose(true);
            TelemetryRecorder?.Dispose();
            TelemetryRecorder = null;
        }
    }
}

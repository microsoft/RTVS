using System;
using System.Collections.Generic;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Diagnostics;
using Microsoft.Common.Core.Telemetry;
using Microsoft.VisualStudio.Telemetry;

namespace Microsoft.VisualStudio.R.Package.Telemetry {
    /// <summary>
    /// Implements telemetry recording in Visual Studio environment
    /// </summary>
    internal sealed class VsTelemetryRecorder : ITelemetryRecorder {
        private TelemetrySession session;
        private static Lazy<VsTelemetryRecorder> _instance = Lazy.Create(() => new VsTelemetryRecorder());

        private VsTelemetryRecorder() {
            this.session = TelemetryService.DefaultSession;
        }

        public static ITelemetryRecorder Current {
            get {
                return _instance.Value;
            }
        }

        #region ITelemetryRecorder
        /// <summary>
        /// True if telemetry is actually being recorder
        /// </summary>
        public bool IsEnabled {
            get { return this.session.IsOptedIn; }
        }

        public bool CanCollectPrivateInformation {
            get { return this.session.CanCollectPrivateInformation; }
        }

        /// <summary>
        /// Records a simple event without parameters.
        /// </summary>
        public void RecordEvent(string eventName) {
            if (this.IsEnabled) {
                this.session.PostEvent(new TelemetryEvent(eventName));
            }
        }

        /// <summary>
        /// Records event with a single parameter
        /// </summary>
        public void RecordEvent(string eventName, string parameterName, object parameterValue) {
            if (this.IsEnabled) {
                TelemetryEvent telemetryEvent = new TelemetryEvent(eventName);
                telemetryEvent.Properties[parameterName] = parameterValue;

                this.session.PostEvent(telemetryEvent);
            }
        }

        /// <summary>
        /// Records event with multiple parameters
        /// </summary>
        public void RecordEvent(string eventName, IDictionary<string, object> parameters) {
            if (this.IsEnabled) {
                TelemetryEvent telemetryEvent = new TelemetryEvent(eventName);
                foreach (KeyValuePair<string, object> kvp in parameters) {
                    telemetryEvent.Properties[kvp.Key] = kvp.Value;
                }

                this.session.PostEvent(telemetryEvent);
            }
        }

        /// <summary>
        /// Records telemetry event
        /// </summary>
        /// <param name="telemetryEvent"></param>
        public void RecordEvent(TelemetryEvent telemetryEvent) {
            if (this.IsEnabled) {
                this.session.PostEvent(telemetryEvent);
            }
        }

        /// <summary>
        /// Records telemetry activity
        /// </summary>
        /// <param name="telemetryEvent"></param>
        public void RecordActivity(object telemetryActivity) {
            Check.InvalidOperation(() => !(telemetryActivity is TelemetryActivity));
            if (this.IsEnabled) {
                this.session.PostEvent(telemetryActivity as TelemetryActivity);
            }
        }
        #endregion
    }
}

using System.Collections.Generic;
using Microsoft.Common.Core.Telemetry;
using Microsoft.VisualStudio.Telemetry;

namespace Microsoft.VisualStudio.R.Package.Telemetry {
    /// <summary>
    /// Wrapper of the TelemetryActivity
    /// </summary>
    internal sealed class TelemetryActivityWrapper : ITelemetryActivity {
        private ITelemetryRecorder recorder;
        private TelemetryActivity activity;
        private string propertyNamePrefix;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="recorder">An instance of the recorder</param>
        /// <param name="name">Event name</param>
        /// <param name="propertyNamePrefix">Prefix of datapoints in this project</param>
        public TelemetryActivityWrapper(ITelemetryRecorder recorder, string name, string propertyNamePrefix) {
            this.Properties = new Dictionary<string, object>();
            this.recorder = recorder;
            this.propertyNamePrefix = propertyNamePrefix;

            this.activity = new TelemetryActivity(name);
            this.activity.Start();
        }

        public IDictionary<string, object> Properties { get; private set; }

        /// <summary>
        /// End the activity and report the event
        /// </summary>
        /// <param name="activity">The activity to report</param>
        public void RecordActivity() {
            if (this.activity != null) {
                this.activity.End();

                foreach (var prop in this.Properties) {
                    this.activity.Properties[this.propertyNamePrefix + prop.Key] = prop.Value;
                }

                this.recorder.RecordActivity(this.activity);
                this.activity = null;
            }
        }

        /// <summary>
        /// End the activity but will not report the event
        /// </summary>
        public void CancelActivity() {
            if (this.activity != null) {
                this.activity.End();
                this.activity = null;
            }
        }

        /// <summary>
        /// Set a PII (personally identifiable information) property in the Properties dictionary.
        /// </summary>
        /// <param name="key">Key in the dictionary.</param>
        /// <param name="value">Value to hash for external users.</param>
        public void SetPiiProperty(string key, object value) {
            this.Properties[key] = new TelemetryPiiProperty(value);
        }
    }
}

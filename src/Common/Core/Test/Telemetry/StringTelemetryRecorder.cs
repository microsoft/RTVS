using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Common.Core.Telemetry;
using Microsoft.VisualStudio.Telemetry;

namespace Microsoft.Common.Core.Tests.Telemetry {

    [ExcludeFromCodeCoverage]
    public sealed class StringTelemetryRecorder : ITelemetryRecorder, ITelemetryTestSupport {
        private StringBuilder stringBuilder = new StringBuilder();

        #region ITelemetryRecorder
        public bool IsEnabled {
            get { return true; }
        }

        public bool CanCollectPrivateInformation {
            get { return true; }
        }

        public void RecordEvent(string eventName, object parameters = null) {
            this.stringBuilder.AppendLine(eventName);
            if (parameters != null) {
                WriteDictionary(DictionaryExtension.FromAnonymousObject(parameters));
            }
        }

        public void RecordActivity(object telemetryActivity) {
            TelemetryActivity activity = telemetryActivity as TelemetryActivity;
            this.stringBuilder.AppendLine(activity.Name);
            if (activity.HasProperties) {
                WriteDictionary(activity.Properties);
            }
        }

        public string SerializeSession() {
            return string.Empty;
        }

        public void Dispose() {
        }
        #endregion

        #region ITelemetryTestSupport
        public void Reset() {
            this.stringBuilder.Clear();
        }

        public string SessionLog {
            get { return this.stringBuilder.ToString(); }
        }
        #endregion

        private void WriteDictionary(IDictionary<string, object> dict) {
            foreach (KeyValuePair<string, object> kvp in dict) {
                WriteProperty(kvp.Key, kvp.Value);
            }
        }

        private void WriteProperty(string name, object value) {
            this.stringBuilder.Append('\t');
            this.stringBuilder.Append(name);
            this.stringBuilder.Append(" : ");
            this.stringBuilder.AppendLine(value.ToString());
        }
    }
}

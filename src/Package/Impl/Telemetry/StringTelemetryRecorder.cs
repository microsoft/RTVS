// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Telemetry;
using Microsoft.VisualStudio.Telemetry;

namespace Microsoft.VisualStudio.R.Package.Telemetry {

    /// <summary>
    /// Records telemetry events into a string. The resulting log can be used
    /// for testing or for submitting telemetry as a file rather than via
    /// VS telemetry Web service.
    /// </summary>
    public sealed class StringTelemetryRecorder : ITelemetryRecorder, IDisposable {
        private StringBuilder _stringBuilder = new StringBuilder();

        #region ITelemetryRecorder
        public bool IsEnabled {
            get { return true; }
        }

        public bool CanCollectPrivateInformation {
            get { return true; }
        }

        public void RecordEvent(string eventName, object parameters = null) {
            _stringBuilder.AppendLine(eventName);
            if (parameters != null) {
                if (parameters is string) {
                    WriteProperty("Value", parameters.ToString());
                } else {
                    WriteDictionary(DictionaryExtensions.FromAnonymousObject(parameters));
                }
            }
        }

        public void RecordActivity(object telemetryActivity) {
            TelemetryActivity activity = telemetryActivity as TelemetryActivity;
            _stringBuilder.AppendLine(activity.Name);
            if (activity.HasProperties) {
                WriteDictionary(activity.Properties);
            }
        }
        #endregion

        #region ITelemetryLog
        public void Reset() {
            _stringBuilder.Clear();
        }

        public string SessionLog {
            get { return _stringBuilder.ToString(); }
        }
        #endregion

        public string FileName { get; private set; }

        public void Dispose() {
            try {
                FileName = Path.Combine(Path.GetTempPath(), string.Format(CultureInfo.InvariantCulture, "RTVS_Telemetry_{0}.log", DateTime.Now.ToFileTimeUtc()));
                using (var sw = new StreamWriter(FileName)) {
                    sw.Write(_stringBuilder.ToString());
                }
            } catch (IOException) { }
        }

        private void WriteDictionary(IDictionary<string, object> dict) {
            foreach (KeyValuePair<string, object> kvp in dict) {
                WriteProperty(kvp.Key, kvp.Value);
            }
        }

        private void WriteProperty(string name, object value) {
            _stringBuilder.Append('\t');
            _stringBuilder.Append(name);
            _stringBuilder.Append(" : ");
            _stringBuilder.AppendLine(value.ToString());
        }
    }
}

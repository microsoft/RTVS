// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
        private TelemetrySession _session;
        private static Lazy<VsTelemetryRecorder> _instance = Lazy.Create(() => new VsTelemetryRecorder());

        private VsTelemetryRecorder() {
            _session = TelemetryService.DefaultSession;
        }

        public static ITelemetryRecorder Current => _instance.Value;

        #region ITelemetryRecorder
        /// <summary>
        /// True if telemetry is actually being recorder
        /// </summary>
        public bool IsEnabled  => _session.IsOptedIn;
        public bool CanCollectPrivateInformation => _session.CanCollectPrivateInformation;

        /// <summary>
        /// Records event with parameters
        /// </summary>
        public void RecordEvent(string eventName, object parameters = null) {
            if (this.IsEnabled) {
                TelemetryEvent telemetryEvent = new TelemetryEvent(eventName);
                if (parameters != null) {
                    var stringParameter = parameters as string;
                    if (stringParameter != null) {
                        telemetryEvent.Properties["Value"] = stringParameter;
                    } else {
                        IDictionary<string, object> dict = DictionaryExtensions.FromAnonymousObject(parameters);
                        foreach (KeyValuePair<string, object> kvp in dict) {
                            telemetryEvent.Properties[kvp.Key] = kvp.Value;
                        }
                    }
                }
                _session.PostEvent(telemetryEvent);
            }
        }

        /// <summary>
        /// Records telemetry event
        /// </summary>
        /// <param name="telemetryEvent"></param>
        public void RecordEvent(TelemetryEvent telemetryEvent) {
            if (this.IsEnabled) {
                _session.PostEvent(telemetryEvent);
            }
        }

        /// <summary>
        /// Records telemetry activity
        /// </summary>
        /// <param name="telemetryEvent"></param>
        public void RecordActivity(object telemetryActivity) {
            Check.InvalidOperation(() => !(telemetryActivity is TelemetryActivity));
            if (this.IsEnabled) {
                _session.PostEvent(telemetryActivity as TelemetryActivity);
            }
        }
        #endregion

        public void Dispose() { }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Common.Core.Telemetry;
using Newtonsoft.Json;

namespace Microsoft.Common.Core.Test.Telemetry {
    /// <summary>
    /// Records telemetry events into file. Typically used in test
    /// scenarios or when remote service is not available. In the latter
    /// case telemetry instead may be collected in disk files and submitted
    /// as part of the user feedback.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public sealed class FileTelemetryRecorder : ITelemetryRecorder {
        public static string TestLog {
            get {
                string logPath = Path.Combine(Path.GetTempPath(), @"Microsoft\RTVS\RtvsTelemetryEvents.json");
                string telemetryFileDirectory = Path.GetDirectoryName(logPath);

                if (!Directory.Exists(telemetryFileDirectory)) {
                    Directory.CreateDirectory(telemetryFileDirectory);
                }

                return logPath;
            }
        }

        public bool IsEnabled => true;

        public bool CanCollectPrivateInformation => true;

        public void RecordEvent(string eventName, object parameters = null) {
            using (StreamWriter sw = File.AppendText(FileTelemetryRecorder.TestLog)) {
                SimpleTelemetryEvent telemetryEvent = new SimpleTelemetryEvent(eventName);
                telemetryEvent.Properties = DictionaryExtensions.FromAnonymousObject(parameters);

                string json = JsonConvert.SerializeObject(telemetryEvent);
                sw.WriteLine(json);
            }
        }

        public void Dispose() { }
    }
}

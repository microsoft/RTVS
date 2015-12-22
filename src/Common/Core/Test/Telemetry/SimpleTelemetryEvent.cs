using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Common.Core.Tests.Telemetry {
    internal sealed class SimpleTelemetryEvent {
        public SimpleTelemetryEvent(string eventName) {
            this.Name = eventName;
        }

        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Properties")]
        public IDictionary<string, object> Properties { get; set; }
    }
}


using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Microsoft.Common.Core.Test.Telemetry {
    internal sealed class SimpleTelemetryEvent {
        [ExcludeFromCodeCoverage]
        public SimpleTelemetryEvent(string eventName) {
            this.Name = eventName;
        }

        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Properties")]
        public IDictionary<string, object> Properties { get; set; }
    }
}


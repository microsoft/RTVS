using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Microsoft.Common.Core.Test.Telemetry {
    [ExcludeFromCodeCoverage]
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


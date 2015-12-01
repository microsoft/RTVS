using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Common.Core.Test.Telemetry {
    [ExcludeFromCodeCoverage]
    public class StringTelemetryRecorderTests {
        [CompositeTest]
        [InlineData("event")]
        public void StringTelemetryRecorder_SimpleEventTest(string eventName) {
            var telemetryRecorder = new StringTelemetryRecorder();
            telemetryRecorder.RecordEvent(eventName);
            string log = telemetryRecorder.SessionLog;
            log.Should().Be(eventName + "\r\n");
        }

        [CompositeTest]
        [InlineData("event", "parameter", "value")]
        public void StringTelemetryRecorder_EventWithParametersTest(string eventName, string parameter, string value) {
            var telemetryRecorder = new StringTelemetryRecorder();
            telemetryRecorder.RecordEvent(eventName, parameter, value);
            string log = telemetryRecorder.SessionLog;
            log.Should().Be(eventName + "\r\n\t" + parameter + " : " + value + "\r\n");
        }

        [CompositeTest]
        [InlineData("event", "parameter1", "value1", "parameter2", "value2")]
        public void StringTelemetryRecorder_EventWithDictionaryTest(string eventName, string parameter1, string value1, string parameter2, string value2) {
            var telemetryRecorder = new StringTelemetryRecorder();
            telemetryRecorder.RecordEvent(eventName, new Dictionary<string, object>() { { parameter1, value1 }, { parameter2, value2 } });
            string log = telemetryRecorder.SessionLog;
            log.Should().Be(eventName + "\r\n\t" + parameter1 + " : " + value1 + "\r\n\t" + parameter2 + " : " + value2 + "\r\n");
        }
    }
}

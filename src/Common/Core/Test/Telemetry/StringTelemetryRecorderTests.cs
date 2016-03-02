// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
            var telemetryRecorder = new TestTelemetryRecorder();
            telemetryRecorder.RecordEvent(eventName);

            string log = telemetryRecorder.SessionLog;
            log.Should().Be(eventName + "\r\n");
        }

        [CompositeTest]
        [InlineData("event", "parameter1", "value1", "parameter2", "value2")]
        public void StringTelemetryRecorder_EventWithDictionaryTest(string eventName, string parameter1, string value1, string parameter2, string value2) {
            var telemetryRecorder = new TestTelemetryRecorder();
            telemetryRecorder.RecordEvent(eventName, new Dictionary<string, object>() { { parameter1, value1 }, { parameter2, value2 } });

            string log = telemetryRecorder.SessionLog;
            log.Should().Be(eventName + "\r\n\t" + parameter1 + " : " + value1 + "\r\n\t" + parameter2 + " : " + value2 + "\r\n");
        }

        [CompositeTest]
        [InlineData("event")]
        public void StringTelemetryRecorder_EventWithAnonymousCollectionTest(string eventName) {
            var telemetryRecorder = new TestTelemetryRecorder();
            telemetryRecorder.RecordEvent(eventName, new { parameter1 = "value1", parameter2 = "value2" });

            string log = telemetryRecorder.SessionLog;
            log.Should().Be(eventName + "\r\n\tparameter1 : value1\r\n\tparameter2 : value2\r\n");
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Telemetry;
using Microsoft.UnitTests.Core.XUnit;
using Xunit;

namespace Microsoft.Common.Core.Test.Telemetry {
    [ExcludeFromCodeCoverage]
    public class TelemetryTestServiceTests {
        [Test]
        public void TelemetryTestService_DefaultPrefixConstructorTest() {
            var telemetryService = new TelemetryTestService();
            telemetryService.EventNamePrefix.Should().Be(TelemetryTestService.EventNamePrefixString);
            telemetryService.PropertyNamePrefix.Should().Be(TelemetryTestService.PropertyNamePrefixString);
        }

        [CompositeTest]
        [InlineData("Event/Prefix/", "Property.Prefix.")]
        public void TelemetryTestService_CustomPrefixConstructorTest(string eventPrefix, string propertyPrefix) {
            var telemetryService = new TelemetryTestService(eventPrefix, propertyPrefix);
            telemetryService.EventNamePrefix.Should().Be(eventPrefix);
            telemetryService.PropertyNamePrefix.Should().Be(propertyPrefix);
        }

        [CompositeTest]
        [InlineData(TelemetryArea.Options, "event")]
        public void TelemetryTestService_SimpleEventTest(TelemetryArea area, string eventName) {
            var telemetryService = new TelemetryTestService();
            telemetryService.ReportEvent(area, eventName);
            string log = telemetryService.SessionLog;
            log.Should().Be(TelemetryTestService.EventNamePrefixString + area.ToString() + "/" + eventName + "\r\n");
        }

        [CompositeTest]
        [InlineData(TelemetryArea.Options, "event")]
        public void TelemetryTestService_EventWithParametersTest(TelemetryArea area, string eventName) {
            var telemetryService = new TelemetryTestService();
            telemetryService.ReportEvent(area, eventName, new { parameter = "value" });
            string log = telemetryService.SessionLog;
            log.Should().Be(TelemetryTestService.EventNamePrefixString + area.ToString() + "/" + eventName +
                            "\r\n\t" + TelemetryTestService.PropertyNamePrefixString + area.ToString() + ".parameter : value\r\n");
        }
    }
}

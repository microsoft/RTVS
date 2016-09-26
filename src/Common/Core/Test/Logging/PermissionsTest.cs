// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Telemetry;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.Win32;
using NSubstitute;
using Xunit;

namespace Microsoft.Common.Core.Test.Logging {
    [ExcludeFromCodeCoverage]
    [Category.Logging]
    public class PermissionsTest {
        [CompositeTest]
        [InlineData(true, null, null, LogLevel.Traffic, true)]
        [InlineData(false, null, null, LogLevel.Minimal, false)]
        [InlineData(true, (int)LogLevel.Normal, null, LogLevel.Normal, true)]
        [InlineData(false, (int)LogLevel.Normal, null, LogLevel.Normal, false)]
        [InlineData(true, (int)LogLevel.Minimal, 0, LogLevel.Minimal, false)]
        [InlineData(false, (int)LogLevel.Traffic, 1, LogLevel.Traffic, true)]
        [InlineData(true, -1, -1, (int)LogLevel.Traffic, true)]
        [InlineData(false, 42, 42, (int)LogLevel.Minimal, false)]
        public void Overrides(bool telemetryEnabled, int? logLevel, int? feedbackSetting, LogLevel expectedMaxLogLevel, bool expectedFeedback) {
            var constants = Substitute.For<IApplicationConstants>();
            constants.LocalMachineHive.Returns("rtvs");

            var rtvsKey = Substitute.For<IRegistryKey>(); ;
            rtvsKey.GetValue(LoggingPermissions.LogLevelValueName).Returns(logLevel);
            rtvsKey.GetValue(LoggingPermissions.FeedbackValueName).Returns(feedbackSetting);

            var hklm = Substitute.For<IRegistryKey>();
            hklm.OpenSubKey("rtvs").Returns(rtvsKey);

            var registry = Substitute.For<IRegistry>();
            registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).Returns(hklm);

            var telemetry = Substitute.For<ITelemetryService>();
            telemetry.IsEnabled.Returns(telemetryEnabled);

            var permissions = new LoggingPermissions(constants, telemetry, registry);
            permissions.MaxLogLevel.Should().Be(expectedMaxLogLevel);
            permissions.IsFeedbackPermitted.Should().Be(expectedFeedback);

            if (logLevel.HasValue) {
                var values = Enum.GetValues(typeof(LogLevel));
                foreach (var v in values) {
                    permissions.Current = (LogLevel)v;
                    ((int)permissions.Current).Should().BeGreaterOrEqualTo((int)LogLevel.None);
                    ((int)permissions.Current).Should().BeLessOrEqualTo((int)permissions.MaxLogLevel);
                }
            }
        }
    }
}

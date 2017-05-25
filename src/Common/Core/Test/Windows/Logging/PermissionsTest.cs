// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Telemetry;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.Win32;
using NSubstitute;
using Xunit;

namespace Microsoft.Windows.Core.Test.Logging {
    [ExcludeFromCodeCoverage]
    [Category.Logging]
    public class PermissionsTest {
        [CompositeTest]
        [InlineData(true, null, null, LogVerbosity.Traffic, true)]
        [InlineData(false, null, null, LogVerbosity.Minimal, false)]
        [InlineData(true, (int)LogVerbosity.Normal, null, LogVerbosity.Normal, true)]
        [InlineData(false, (int)LogVerbosity.Normal, null, LogVerbosity.Normal, false)]
        [InlineData(true, (int)LogVerbosity.Minimal, 0, LogVerbosity.Minimal, false)]
        [InlineData(false, (int)LogVerbosity.Traffic, 1, LogVerbosity.Traffic, true)]
        [InlineData(true, -1, -1, (int)LogVerbosity.Traffic, true)]
        [InlineData(false, 42, 42, (int)LogVerbosity.Minimal, false)]
        public void Overrides(bool telemetryEnabled, int? logVerbosity, int? feedbackSetting, LogVerbosity expectedMaxLogLevel, bool expectedFeedback) {
            var rtvsKey = Substitute.For<IRegistryKey>();
            rtvsKey.GetValue(LoggingPermissions.LogVerbosityValueName).Returns(logVerbosity);
            rtvsKey.GetValue(LoggingPermissions.FeedbackValueName).Returns(feedbackSetting);

            var hklm = Substitute.For<IRegistryKey>();
            hklm.OpenSubKey("rtvs").Returns(rtvsKey);

            var registry = Substitute.For<IRegistry>();
            registry.LocalMachineHive.Returns("rtvs");
            registry.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).Returns(hklm);

            var telemetry = Substitute.For<ITelemetryService>();
            telemetry.IsEnabled.Returns(telemetryEnabled);

            var services = new ServiceManager()
                .AddService(telemetry)
                .AddService(registry);

            var permissions = new LoggingPermissions(services);
            permissions.MaxVerbosity.Should().Be(expectedMaxLogLevel);
            permissions.IsFeedbackPermitted.Should().Be(expectedFeedback);

            if (logVerbosity.HasValue) {
                var values = Enum.GetValues(typeof(LogVerbosity));
                foreach (var v in values) {
                    permissions.CurrentVerbosity = (LogVerbosity)v;
                    ((int)permissions.CurrentVerbosity).Should().BeGreaterOrEqualTo((int)LogVerbosity.None);
                    ((int)permissions.CurrentVerbosity).Should().BeLessOrEqualTo((int)permissions.MaxVerbosity);
                }
            }
        }
    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Test.Telemetry;
using Microsoft.Language.Editor.Test.Settings;
using Microsoft.R.Components.Test.Stubs;
using Microsoft.R.Editor;
using Microsoft.R.Editor.Functions;
using Microsoft.R.Editor.Settings;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Telemetry;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Mocks;
using NSubstitute;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Telemetry {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    [Category.Telemetry]
    public class TelemetryTest {
        private readonly IPackageIndex _packageIndex;
        private readonly IREditorSettings _settings;

        public TelemetryTest() {
            _packageIndex = Substitute.For<IPackageIndex>();

            var package1 = Substitute.For<IPackageInfo>();
            package1.Name.Returns("base");
            var package2 = Substitute.For<IPackageInfo>();
            package1.Name.Returns("user_package");

            _packageIndex.Packages.Returns(new IPackageInfo[] { package1, package2 });

            _settings = new REditorSettings(new TestSettingsStorage());
        }

        [Test]
        public void ReportConfiguration() {
            var svc = new TelemetryTestService();
            string log;
            using (var t = new RtvsTelemetry(_packageIndex, new RSettingsStub(), _settings, svc)) {
                t.ReportConfiguration();
                log = svc.SessionLog;
            }

            log.Length.Should().BeGreaterThan(0);
            log.Should().Contain(TelemetryTestService.EventNamePrefixString);
            log.Should().Contain(RtvsTelemetry.ConfigurationEvents.RPackages);
        }

        [Test]
        public void ReportSettings() {
            var svc = new TelemetryTestService();
            string log;
            using (var t = new RtvsTelemetry(_packageIndex, new RSettingsStub(), _settings, svc)) {
                t.ReportSettings();
                log = svc.SessionLog;
            }

            log.Length.Should().BeGreaterThan(0);
            log.Should().Contain("Cran");
            log.Should().Contain("LoadRData");
            log.Should().Contain("SaveRData");
            log.Should().Contain("RCommandLineArguments");
            log.Should().Contain("MultilineHistorySelection");
            log.Should().Contain("AlwaysSaveHistory");
            log.Should().Contain("AutoFormat");
            log.Should().Contain("CommitOnEnter");
            log.Should().Contain("CommitOnSpace");
            log.Should().Contain("FormatOnPaste");
            log.Should().Contain("SendToReplOnCtrlEnter");
            log.Should().Contain("ShowCompletionOnFirstChar");
            log.Should().Contain("SignatureHelpEnabled");
            log.Should().Contain("CompletionEnabled");
            log.Should().Contain("SyntaxCheckInRepl");
            log.Should().Contain("PartialArgumentNameMatch");
        }

        [Test]
        public void ReportWindowLayout() {
            var svc = new TelemetryTestService();
            var shell = new VsUiShellMock();
            Guid g = new Guid("6B72640E-99F8-40A5-BCDB-BB8CF250A1B5");
            Guid p1 = new Guid("E5815AEF-4D98-4BC4-84A0-5DF62ED0755D");
            Guid p2 = new Guid("22130C87-7D87-4C41-9BC0-14BFA3261DA8");
            Guid p3 = new Guid("B7AF6C09-BC6F-41F1-AF1A-B67D267F1C0E");

            IVsWindowFrame frame;
            shell.CreateToolWindow(0, 1, null, ref g, ref p1, ref g, null, "Window#1", null, out frame);
            shell.CreateToolWindow(0, 2, null, ref g, ref p2, ref g, null, "Window#2", null, out frame);
            shell.CreateToolWindow(0, 3, null, ref g, ref p3, ref g, null, "Window#3", null, out frame);

            string log;
            using (var t = new RtvsTelemetry(_packageIndex, new RSettingsStub(), _settings, svc)) {
                t.ReportWindowLayout(shell);
                log = svc.SessionLog;
            }

            log.Length.Should().BeGreaterThan(0);
            log.Should().Contain("Window#1");
            log.Should().Contain("Window#2");
            log.Should().Contain("Window#3");
            log.Should().Contain("100");
        }
    }
}

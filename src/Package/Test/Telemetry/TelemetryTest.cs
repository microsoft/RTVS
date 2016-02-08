using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Test.Telemetry;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Telemetry;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Mocks;

namespace Microsoft.VisualStudio.R.Package.Test.Telemetry {
    [ExcludeFromCodeCoverage]
    public class TelemetryTest {
        [Test]
        [Category.Telemetry]
        public void ReportConfiguration() {
            var svc = new TelemetryTestService();
            string log;
            using (var t = new RtvsTelemetry(svc)) {
                t.ReportConfiguration();
                log = svc.SessionLog;
            }

            log.Length.Should().BeGreaterThan(0);
            log.Should().Contain(TelemetryTestService.EventNamePrefixString);
            log.Should().Contain(RtvsTelemetry.ConfigurationEvents.RBasePackages);
            log.Should().Contain(RtvsTelemetry.ConfigurationEvents.RUserPackages);
        }

        [Test]
        [Category.Telemetry]
        public void ReportSettings() {
            var svc = new TelemetryTestService();
            string log;
            using (var t = new RtvsTelemetry(svc)) {
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
        [Category.Telemetry]
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
            using (var t = new RtvsTelemetry(svc)) {
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

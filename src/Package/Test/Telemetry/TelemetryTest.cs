using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Test.Telemetry;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Telemetry;

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
    }
}

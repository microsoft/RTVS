// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.OS;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Feedback;
using Microsoft.VisualStudio.Shell;
using NSubstitute;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Commands {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class FeedbackCommandTest {
        [Test]
        public void ReportIssue() {
            var lp = Substitute.For<ILoggingPermissions>();
            var pss = Substitute.For<IProcessServices>();

            var cmd = new ReportIssueCommand(lp, pss);
            TestStatus(cmd, lp);
        }

        [Test]
        public void SendFrown() {
            var lp = Substitute.For<ILoggingPermissions>();
            var pss = Substitute.For<IProcessServices>();
            var log = Substitute.For<IActionLog>();

            var cmd = new SendFrownCommand(lp, pss, log);
            TestStatus(cmd, lp);
        }

        [Test]
        public void SendSmile() {
            var lp = Substitute.For<ILoggingPermissions>();
            var pss = Substitute.For<IProcessServices>();
            var log = Substitute.For<IActionLog>();

            var cmd = new SendSmileCommand(lp, pss, log);
            TestStatus(cmd, lp);
        }

        private void TestStatus(OleMenuCommand cmd, ILoggingPermissions lp) {
            lp.IsFeedbackPermitted.Returns(false);
            cmd.Visible.Should().BeFalse();

            lp.IsFeedbackPermitted.Returns(true);
            cmd.Visible.Should().BeTrue();
        }
    }
}
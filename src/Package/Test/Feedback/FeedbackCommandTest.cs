// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Feedback;
using Microsoft.VisualStudio.Shell;
using NSubstitute;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Commands {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class FeedbackCommandTest {
        private readonly ILoggingPermissions _lp;
        private readonly ICoreShell _coreShell;

        public FeedbackCommandTest() {
            _lp = Substitute.For<ILoggingPermissions>();
            _coreShell = new TestCoreShell(null, null, _lp);
        }

        [Test]
        public void ReportIssue() {
            var cmd = new ReportIssueCommand(_coreShell);
            TestStatus(cmd);
        }

        [Test]
        public void SendFrown() {
            var cmd = new SendFrownCommand(_coreShell);
            TestStatus(cmd);
        }

        [Test]
        public void SendSmile() {
            var cmd = new SendSmileCommand(_coreShell);
            TestStatus(cmd);
        }

        private void TestStatus(OleMenuCommand cmd) {
            _lp.IsFeedbackPermitted.Returns(false);
            cmd.Should().BeInvisibleAndDisabled();

            _lp.IsFeedbackPermitted.Returns(true);
            cmd.Should().BeEnabled();
        }
    }
}
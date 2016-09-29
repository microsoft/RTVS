// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Logging;
using Microsoft.Common.Core.Services;
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
        private readonly ICoreServices _services;

        public FeedbackCommandTest() {
            _lp = Substitute.For<ILoggingPermissions>();
            _services = TestCoreServices.CreateSubstitute();
        }

        [Test]
        public void ReportIssue() {
            var cmd = new ReportIssueCommand(_lp, _services.ProcessServices);
            TestStatus(cmd);
        }

        [Test]
        public void SendFrown() {
            var cmd = new SendFrownCommand(_lp, _services);
            TestStatus(cmd);
        }

        [Test]
        public void SendSmile() {
            var cmd = new SendSmileCommand(_lp, _services);
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
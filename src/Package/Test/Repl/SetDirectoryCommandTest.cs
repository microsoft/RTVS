// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client.Test.Mocks;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.R.Package.ProjectSystem;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using NSubstitute;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    public class SetDirectoryCommandTest {
        [Test]
        [Category.Repl]
        public void SetDirectoryToSourceTest() {
            var session = new RSessionMock();
            var workflow = Substitute.For<IRInteractiveWorkflow>();
            workflow.RSession.Returns(session);

            var document = Substitute.For<ITextDocument>();
            document.FilePath.Returns(@"c:\dir1\dir2\file.r");

            var tb = new TextBufferMock(string.Empty, RContentTypeDefinition.ContentType);
            tb.Properties.AddProperty(typeof(ITextDocument), document);

            var tracker = Substitute.For<IActiveWpfTextViewTracker>();
            tracker.LastActiveTextView.Returns((IWpfTextView)null);

            var cmd = new SetDirectoryToSourceCommand(workflow, tracker);
            var status = cmd.OleStatus;
            cmd.Enabled.Should().BeFalse();
            cmd.Supported.Should().BeTrue();

            tracker = Substitute.For<IActiveWpfTextViewTracker>();
            tracker.LastActiveTextView.Returns(new WpfTextViewMock(tb));

            cmd = new SetDirectoryToSourceCommand(workflow, tracker);
            status = cmd.OleStatus;
            cmd.Enabled.Should().BeFalse();

            session.IsHostRunning = true;
            status = cmd.OleStatus;
            cmd.Enabled.Should().BeTrue();

            cmd.Invoke();
            session.LastExpression.Should().Be("setwd('c:/dir1/dir2')\n");

            session.IsRemote = true;
            status = cmd.OleStatus;
            cmd.Enabled.Should().BeFalse();
        }

        [Test]
        [Category.Repl]
        public void SetDirectoryToProjectTest() {
            var session = new RSessionMock();
            session.IsHostRunning = true;

            var workflow = Substitute.For<IRInteractiveWorkflow>();
            workflow.RSession.Returns(session);

            var pss = Substitute.For<IProjectSystemServices>();
            pss.GetSelectedProject<IVsProject>().Returns((IVsProject)null);
            pss.GetActiveProject().Returns((EnvDTE.Project)null);

            var cmd = new SetDirectoryToProjectCommand(workflow, pss);
            var status = cmd.OleStatus;
            cmd.Enabled.Should().BeFalse();
            cmd.Supported.Should().BeTrue();

            var proj = Substitute.For<IVsProject>();
            string value;
            proj
                .GetMkDocument((uint)VSConstants.VSITEMID.Root, out value)
                .Returns(x => {
                    x[1] = @"c:\dir1\dir2\file.rproj";
                    return VSConstants.S_OK;
                });

            pss = Substitute.For<IProjectSystemServices>();
            pss.GetSelectedProject<IVsProject>().Returns(proj);

            var proj2 = Substitute.For<EnvDTE.Project>();
            pss.GetActiveProject().Returns(proj2);

            cmd = new SetDirectoryToProjectCommand(workflow, pss);
            status = cmd.OleStatus;
            cmd.Enabled.Should().BeTrue();
            cmd.Supported.Should().BeTrue();
            cmd.Invoke();

            session.LastExpression.Should().Be("setwd('c:/dir1/dir2')\n");
        }
    }
}

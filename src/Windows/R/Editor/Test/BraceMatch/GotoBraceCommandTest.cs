// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.Languages.Editor.BraceMatch;
using Microsoft.Languages.Editor.Controllers.Constants;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.R.Editor.Test.BraceMatch {
    [ExcludeFromCodeCoverage]
    public class GotoBraceCommandTest {
        private readonly ICoreShell _shell;

        public GotoBraceCommandTest(REditorShellProviderFixture shellProvider) {
            _shell = shellProvider.CoreShell;
        }

        [Test]
        [Category.Languages.Core]
        public void GotoBrace() {
            string content = "if(x<1) {x<-2}";
            ITextBuffer textBuffer = new TextBufferMock(content, "R");
            ITextView textView = new TextViewMock(textBuffer);
            var command = new GotoBraceCommand(textView, textBuffer, _shell);
            object o = new object();

            var status = command.Status(VSConstants.VSStd2K, (int) VSConstants.VSStd2KCmdID.GOTOBRACE);
            status.Should().Be(CommandStatus.SupportedAndEnabled);

            status = command.Status(VSConstants.VSStd2K, (int) VSConstants.VSStd2KCmdID.GOTOBRACE_EXT);
            status.Should().Be(CommandStatus.SupportedAndEnabled);

            textView.Caret.MoveTo(new SnapshotPoint(textBuffer.CurrentSnapshot, 2));

            command.Invoke(VSConstants.VSStd2K, (int) VSConstants.VSStd2KCmdID.GOTOBRACE, null, ref o);
            textView.Caret.Position.BufferPosition.Position.Should().Be(7);

            command.Invoke(VSConstants.VSStd2K, (int) VSConstants.VSStd2KCmdID.GOTOBRACE, null, ref o);
            textView.Caret.Position.BufferPosition.Position.Should().Be(2);

            textView.Caret.MoveTo(new SnapshotPoint(textBuffer.CurrentSnapshot, 8));

            command.Invoke(VSConstants.VSStd2K, (int) VSConstants.VSStd2KCmdID.GOTOBRACE, null, ref o);
            textView.Caret.Position.BufferPosition.Position.Should().Be(14);

            command.Invoke(VSConstants.VSStd2K, (int) VSConstants.VSStd2KCmdID.GOTOBRACE, null, ref o);
            textView.Caret.Position.BufferPosition.Position.Should().Be(8);
        }
    }
}
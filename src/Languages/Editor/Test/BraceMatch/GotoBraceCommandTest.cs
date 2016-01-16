using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using FluentAssertions;
using Microsoft.Languages.Editor.BraceMatch;
using Microsoft.Languages.Editor.Controller.Constants;
using Microsoft.Languages.Editor.Services;
using Microsoft.Languages.Editor.Shell;
using Microsoft.Languages.Editor.Tasks;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Xunit;

namespace Microsoft.Languages.Editor.Test.BraceMatch {
    [ExcludeFromCodeCoverage]
    public class GotoBraceCommandTest {
        [Test]
        [Category.Languages.Core]
        public void GotoBrace() {
            string content = "if(x<1) {x<-2}";
            ITextBuffer textBuffer = new TextBufferMock(content, "R");
            ITextView textView = new TextViewMock(textBuffer);
            var command = new GotoBraceCommand(textView, textBuffer);
            object o = new object();

            var status = command.Status(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.GOTOBRACE);
            status.Should().Be(CommandStatus.SupportedAndEnabled);

            status = command.Status(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.GOTOBRACE_EXT);
            status.Should().Be(CommandStatus.SupportedAndEnabled);

            textView.Caret.MoveTo(new SnapshotPoint(textBuffer.CurrentSnapshot, 2));

            command.Invoke(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.GOTOBRACE, null, ref o);
            textView.Caret.Position.BufferPosition.Position.Should().Be(7);

            command.Invoke(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.GOTOBRACE, null, ref o);
            textView.Caret.Position.BufferPosition.Position.Should().Be(2);

            textView.Caret.MoveTo(new SnapshotPoint(textBuffer.CurrentSnapshot, 8));

            command.Invoke(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.GOTOBRACE, null, ref o);
            textView.Caret.Position.BufferPosition.Position.Should().Be(14);

            command.Invoke(VSConstants.VSStd2K, (int)VSConstants.VSStd2KCmdID.GOTOBRACE, null, ref o);
            textView.Caret.Position.BufferPosition.Position.Should().Be(8);
        }
    }
}
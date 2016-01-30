using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Languages.Editor;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Mocks;
using Microsoft.R.Host.Client.Test.Script;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Microsoft.VisualStudio.R.Package.Repl.Workspace;
using Microsoft.VisualStudio.R.Package.Test.Mocks;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Mocks;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.R.Package.Test.Commands {
    [ExcludeFromCodeCoverage]
    public class ReplCommandTest {
        [Test]
        [Category.R.Repl]
        public void InterruptRStatusTest() {
            var debugger = new VsDebuggerMock();
            var sp = new RSessionProviderMock();
            var rw = new ReplWindowMock();
            var command = new InterruptRCommand(rw, sp, debugger);

            command.SetStatus();
            command.Visible.Should().BeFalse();
            command.Enabled.Should().BeFalse();

            rw.IsActive = true;

            command.SetStatus();
            command.Visible.Should().BeTrue();
            command.Enabled.Should().BeFalse();

            var session = sp.GetOrCreate(GuidList.InteractiveWindowRSessionGuid, new RHostClientTestApp());
            session.StartHostAsync(null);

            command.SetStatus();
            command.Visible.Should().BeTrue();
            command.Enabled.Should().BeFalse();

            session.BeginEvaluationAsync();

            command.SetStatus();
            command.Visible.Should().BeTrue();
            command.Enabled.Should().BeTrue();

            debugger.Mode = DBGMODE.DBGMODE_Break;

            command.SetStatus();
            command.Visible.Should().BeTrue();
            command.Enabled.Should().BeFalse();

            debugger.Mode = DBGMODE.DBGMODE_Run;

            command.SetStatus();
            command.Visible.Should().BeTrue();
            command.Enabled.Should().BeTrue();

            command.Handle();

            command.SetStatus();
            command.Visible.Should().BeTrue();
            command.Enabled.Should().BeFalse();

            session.Dispose();

            command.SetStatus();
            command.Visible.Should().BeTrue();
            command.Enabled.Should().BeFalse();
        }

        [Test]
        [Category.R.Repl]
        public void SendToReplTest() {
            string content = "x <- 1\r\ny <- 2\r\n";

            var tb = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            var tv = new TextViewMock(tb);

            var rw = new ReplWindowMock();
            ReplWindow.Current = rw;
            var command = new SendToReplCommand(tv);

            var frame = ReplWindow.FindReplWindowFrame(__VSFINDTOOLWIN.FTW_fFindFirst);
            frame.Should().NotBeNull();
            frame.IsVisible().Should().Be(VSConstants.S_OK);

            Guid group = VSConstants.VsStd11;
            int id = (int)VSConstants.VSStd11CmdID.ExecuteLineInInteractive;
            object o = new object();

            command.Status(group, id).Should().Be(CommandStatus.SupportedAndEnabled);

            frame.Hide();
            frame.IsVisible().Should().Be(VSConstants.S_FALSE);

            command.Invoke(group, id, null, ref o);

            frame.IsVisible().Should().Be(VSConstants.S_OK);
            rw.EnqueuedCode.Should().Be("x <- 1");

            int caretPos = tv.Caret.Position.BufferPosition.Position;
            int lineNum = tb.CurrentSnapshot.GetLineNumberFromPosition(caretPos);
            lineNum.Should().Be(1);

            tv.Selection.Select(new SnapshotSpan(tb.CurrentSnapshot, new Span(0, 1)), false);
            command.Invoke(group, id, null, ref o);
            rw.EnqueuedCode.Should().Be("x");
        }
    }
}

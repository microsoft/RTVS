using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Test.StubBuilders;
using Microsoft.R.Components.Test.Stubs;
using Microsoft.R.Components.Test.Stubs.VisualComponents;
using Microsoft.R.Components.View;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Host.Client.Mocks;
using Microsoft.R.Support.Settings;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.R.Package.Commands.R;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Microsoft.VisualStudio.R.Package.Repl.Workspace;
using Microsoft.VisualStudio.R.Package.Test.Mocks;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.R.Package.Test.Commands {
    [ExcludeFromCodeCoverage]
    public class ReplCommandTest {
        private readonly VsDebuggerModeTracker _debuggerModeTracker;
        private readonly RInteractiveWorkflow _workflow;
        private readonly RInteractiveWorkflowProviderStub _workflowProvider;
        private readonly IInteractiveWindowComponentFactory _componentFactory;

        public ReplCommandTest() {
            var sessionProvider = new RSessionProviderMock();
            var historyProvider = RHistoryProviderBuilder.CreateDefault();
            var contentType = new ContentTypeMock(RContentTypeDefinition.ContentType);
            var activeViewTrackerMock = new ActiveTextViewTrackerMock(string.Empty, RContentTypeDefinition.ContentType);
            _debuggerModeTracker = new VsDebuggerModeTracker();

            _componentFactory = new InteractiveWindowComponentFactoryMock();
            _workflow = new RInteractiveWorkflow(sessionProvider, historyProvider, contentType, activeViewTrackerMock, _debuggerModeTracker, null, RToolsSettings.Current, () => {});
            _workflowProvider = new RInteractiveWorkflowProviderStub(_workflow, _componentFactory);
        }

        [Test]
        [Category.Repl]
        public async Task InterruptRStatusTest() {
            var command = new InterruptRCommand(_workflow, _debuggerModeTracker);
            command.Should().BeInvisibleAndDisabled();

            using (await _workflow.CreateInteractiveWindowAsync(_componentFactory)) {
                command.Should().BeVisibleAndDisabled();

                await _workflow.RSession.BeginEvaluationAsync();
                command.Should().BeVisibleAndEnabled();

                _debuggerModeTracker.OnModeChange(DBGMODE.DBGMODE_Break);
                command.Should().BeVisibleAndDisabled();

                _debuggerModeTracker.OnModeChange(DBGMODE.DBGMODE_Run);
                command.Should().BeVisibleAndEnabled();

                command.Invoke();
                command.Should().BeVisibleAndDisabled();
            }

            command.Should().BeVisibleAndDisabled();
        }

        [Test]
        [Category.Repl]
        public void SendToReplTest() {
            string content = "x <- 1\r\ny <- 2\r\n";

            var tb = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            var tv = new TextViewMock(tb);

            var commandFactory = new VsRCommandFactory(_workflowProvider);
            var command = commandFactory.GetCommands(tv, tb).OfType<SendToReplCommand>()
                .Should().ContainSingle().Which;

            _workflow.ActiveWindow.Should().NotBeNull();

            var textBuffer = _workflow.ActiveWindow.InteractiveWindow.CurrentLanguageBuffer;
            var containerStub = (VisualComponentContainerStub<RInteractiveWindowVisualComponent>)_workflow.ActiveWindow.Container;
            containerStub.IsOnScreen.Should().BeFalse();

            Guid group = VSConstants.VsStd11;
            int id = (int)VSConstants.VSStd11CmdID.ExecuteLineInInteractive;
            object o = new object();

            command.Status(group, id).Should().Be(CommandStatus.SupportedAndEnabled);

            containerStub.IsOnScreen = false;
            command.Invoke(group, id, null, ref o);

            textBuffer.CurrentSnapshot.GetText().Should().Be("x <- 1");

            int caretPos = tv.Caret.Position.BufferPosition.Position;
            int lineNum = tb.CurrentSnapshot.GetLineNumberFromPosition(caretPos);
            lineNum.Should().Be(1);

            tv.Selection.Select(new SnapshotSpan(tb.CurrentSnapshot, new Span(0, 1)), false);
            command.Invoke(group, id, null, ref o);
            textBuffer.CurrentSnapshot.GetText().Should().Be("x");

            _workflow.ActiveWindow.Dispose();
            _workflow.ActiveWindow.Should().BeNull();
        }
    }
}

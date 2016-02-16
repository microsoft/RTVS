using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.Controller;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.InteractiveWorkflow.Implementation;
using Microsoft.R.Components.Test.Fakes.InteractiveWindow;
using Microsoft.R.Components.Test.StubFactories;
using Microsoft.R.Components.Test.Stubs;
using Microsoft.R.Components.Test.Stubs.VisualComponents;
using Microsoft.R.Components.View;
using Microsoft.R.Editor.ContentType;
using Microsoft.R.Host.Client.Mocks;
using Microsoft.R.Support.Settings;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.R.Package.Commands.R;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Microsoft.VisualStudio.R.Package.Repl.Workspace;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Test.Mocks;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.R.Package.Test.Commands {
    [ExcludeFromCodeCoverage]
    public class ReplCommandTest {
        private readonly VsDebuggerModeTracker _debuggerModeTracker;
        private readonly IRInteractiveWorkflow _workflow;
        private readonly IRInteractiveWorkflowProvider _workflowProvider;
        private readonly IInteractiveWindowComponentContainerFactory _componentContainerFactory;

        public ReplCommandTest() {
            var sessionProvider = new RSessionProviderMock();
            var historyProvider = RHistoryProviderStubFactory.CreateDefault();
            var activeViewTrackerMock = new ActiveTextViewTrackerMock(string.Empty, RContentTypeDefinition.ContentType);
            _debuggerModeTracker = new VsDebuggerModeTracker();

            _componentContainerFactory = new InteractiveWindowComponentContainerFactoryMock();
            _workflowProvider = new TestRInteractiveWorkflowProvider(
                sessionProvider, historyProvider, _componentContainerFactory, activeViewTrackerMock, _debuggerModeTracker, VsAppShell.Current, RToolsSettings.Current);
            _workflow = _workflowProvider.GetOrCreate();
        }

        [Test]
        [Category.Repl]
        public async Task InterruptRStatusTest() {
            var command = new InterruptRCommand(_workflow, _debuggerModeTracker);
            command.Should().BeInvisibleAndDisabled();

            using (await UIThreadHelper.Instance.Invoke(() => _workflow.GetOrCreateVisualComponent(_componentContainerFactory))) {
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
        public async Task SendToReplTest() {
            string content = "x <- 1\r\ny <- 2\r\n";

            var tb = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            var tv = new TextViewMock(tb);

            var commandFactory = new VsRCommandFactory(_workflowProvider);
            var commands = UIThreadHelper.Instance.Invoke(() => commandFactory.GetCommands(tv, tb));
            
            await IsTrue(_workflow.ActiveWindow != null);
            _workflow.ActiveWindow.Should().NotBeNull();

            await IsTrue(!_workflow.ActiveWindow.InteractiveWindow.IsInitializing);

            var command = commands.OfType<SendToReplCommand>()
                .Should().ContainSingle().Which;

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

        private async Task IsTrue(bool condition) {
            var attempts = 10;
            while (!condition && attempts-- > 0) {
                await Task.Delay(100);
            }
        }
    }
}

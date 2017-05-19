// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Common.Core.Services;
using Microsoft.Common.Core.Shell;
using Microsoft.Common.Core.Test.Fakes.Shell;
using Microsoft.Common.Core.UI.Commands;
using Microsoft.R.Components.ConnectionManager.Implementation;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Test.Stubs;
using Microsoft.UnitTests.Core.FluentAssertions;
using Microsoft.UnitTests.Core.Threading;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.Editor.Mocks;
using Microsoft.VisualStudio.R.Package.Commands.R;
using Microsoft.VisualStudio.R.Package.Repl.Commands;
using Microsoft.VisualStudio.R.Package.Test.FakeFactories;
using Microsoft.VisualStudio.R.Package.Utilities;
using Microsoft.VisualStudio.Text;
using NSubstitute;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Commands {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class ReplCommandTest : IDisposable {
        private readonly VsDebuggerModeTracker _debuggerModeTracker;
        private readonly IRInteractiveWorkflowVisual _workflow;
        private readonly IRInteractiveWorkflowVisualProvider _workflowProvider;
 
        public ReplCommandTest(IServiceContainer services) {
            var coreShell = services.GetService<ICoreShell>();
            _debuggerModeTracker = new VsDebuggerModeTracker(coreShell);
 
            _workflowProvider = TestRInteractiveWorkflowProviderFactory.Create(
                connectionsProvider: new ConnectionManagerProvider(services),
                debuggerModeTracker: _debuggerModeTracker);
            _workflow = _workflowProvider.GetOrCreate();
        }

        public void Dispose() {
            _workflow?.Dispose();
        }

        [Test]
        [Category.Repl]
        public async Task SendToReplTest() {
            string content = "x <- 1\r\ny <- 2\r\n";

            var editorBuffer = new TextBufferMock(content, RContentTypeDefinition.ContentType);
            var tv = new TextViewMock(editorBuffer);

            var commandFactory = new VsRCommandFactory(_workflowProvider);
            var commands = UIThreadHelper.Instance.Invoke(() => commandFactory.GetCommands(tv, editorBuffer));
            await _workflow.RSession.HostStarted.Should().BeCompletedAsync();

            await UIThreadHelper.Instance.Invoke(() => _workflow.GetOrCreateVisualComponentAsync());
            _workflow.ActiveWindow.Should().NotBeNull();

            var command = commands.OfType<SendToReplCommand>()
                .Should().ContainSingle().Which;

            var replBuffer = _workflow.ActiveWindow.InteractiveWindow.CurrentLanguageBuffer;
            _workflow.ActiveWindow.Container.IsOnScreen.Should().BeFalse();

            Guid group = VSConstants.VsStd11;
            int id = (int)VSConstants.VSStd11CmdID.ExecuteLineInInteractive;
            object o = new object();

            command.Status(group, id).Should().Be(CommandStatus.SupportedAndEnabled);
            command.Invoke(group, id, null, ref o);

            replBuffer.CurrentSnapshot.GetText().Trim().Should().Be("x <- 1");

            int caretPos = tv.Caret.Position.BufferPosition.Position;
            int lineNum = editorBuffer.CurrentSnapshot.GetLineNumberFromPosition(caretPos);
            lineNum.Should().Be(1);

            tv.Selection.Select(new SnapshotSpan(editorBuffer.CurrentSnapshot, new Span(0, 1)), false);
            command.Invoke(group, id, null, ref o);

            ITextSnapshotLine line = replBuffer.CurrentSnapshot.GetLineFromLineNumber(1);
            line.GetText().Trim().Should().Be("x");

            _workflow.ActiveWindow.Dispose();
        }
    }
}
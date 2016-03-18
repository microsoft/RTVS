// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.R.Components.History;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Components.Test.Fakes.Trackers;
using Microsoft.R.Host.Client;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Commands;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Test.Mocks;
using NSubstitute;
using Xunit;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class ProjectCommandsTest {
        private readonly IRInteractiveWorkflowProvider _interactiveWorkflowProvider;

        public ProjectCommandsTest() {
            var sessionProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRSessionProvider>();
            var historyProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRHistoryProvider>();
            var activeTextViewTracker = new ActiveTextViewTrackerMock(string.Empty, string.Empty);
            var debuggerModeTracker = new TestDebuggerModeTracker();
            _interactiveWorkflowProvider = new VsRInteractiveWorkflowProvider(sessionProvider, historyProvider, activeTextViewTracker, debuggerModeTracker);
        }

        [Test]
        [Category.Project]
        public async System.Threading.Tasks.Task CopyItemPath() {
            var node1 = Substitute.For<IProjectTree>();
            var node2 = Substitute.For<IProjectTree>();
            var filePath = @"C:\Temp";
            node1.FilePath.Returns(filePath);
            var nodes1 = ImmutableHashSet.Create(node1);
            var nodes2 = ImmutableHashSet.Create(node1, node2);
            var cmd = new CopyItemPathCommand(_interactiveWorkflowProvider);
            CommandStatusResult csr;

            csr = await cmd.GetCommandStatusAsync(nodes1, 0, false, string.Empty, CommandStatus.Enabled);
            csr.Should().Be(CommandStatusResult.Unhandled);

            csr = await cmd.GetCommandStatusAsync(nodes1, RPackageCommandId.icmdCopyItemPath, false, string.Empty, CommandStatus.Enabled);
            csr.Status.Should().Be(CommandStatus.Enabled | CommandStatus.Supported);

            csr = await cmd.GetCommandStatusAsync(nodes2, RPackageCommandId.icmdCopyItemPath, false, string.Empty, CommandStatus.Enabled);
            csr.Should().Be(CommandStatusResult.Unhandled);

            bool result = await cmd.TryHandleCommandAsync(nodes1, RPackageCommandId.icmdCopyItemPath, false, 0, IntPtr.Zero, IntPtr.Zero);
            result.Should().BeTrue();

            await VsAppShell.Current.DispatchOnMainThreadAsync(() => {
                var contents = Clipboard.GetText();
                contents.Should().Be("\"" + filePath + "\"");
            });
        }
    }
}

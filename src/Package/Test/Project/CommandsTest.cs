// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using FluentAssertions;
using Microsoft.Common.Core.OS;
using Microsoft.R.Components.ConnectionManager;
using Microsoft.R.Components.History;
using Microsoft.R.Components.PackageManager;
using Microsoft.R.Components.Test.Fakes.InteractiveWindow;
using Microsoft.R.Host.Client.Session;
using Microsoft.UnitTests.Core.XUnit;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.ProjectSystem.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Package.Test.FakeFactories;
using NSubstitute;
using Xunit;
using Microsoft.Common.Core.Logging;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Designers;
#else
using Microsoft.VisualStudio.ProjectSystem;
#endif
using static Microsoft.UnitTests.Core.Threading.UIThreadTools;

namespace Microsoft.VisualStudio.R.Package.Test.Repl {
    [ExcludeFromCodeCoverage]
    [Collection(CollectionNames.NonParallel)]
    public class ProjectCommandsTest : IDisposable {
        private readonly TestRInteractiveWorkflowProvider _interactiveWorkflowProvider;

        public ProjectCommandsTest() {
            var sessionProvider = new RSessionProvider(Substitute.For<IActionLog>());
            var connectionsProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IConnectionManagerProvider>();
            var historyProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRHistoryProvider>();
            var packagesProvider = VsAppShell.Current.ExportProvider.GetExportedValue<IRPackageManagerProvider>();
            _interactiveWorkflowProvider = TestRInteractiveWorkflowProviderFactory.Create(nameof(ProjectCommandsTest), sessionProvider, connectionsProvider, historyProvider, packagesProvider);
        }

        public void Dispose() {
            _interactiveWorkflowProvider.Dispose();
        }

        [Test]
        [Category.Project]
        public async Task CopyItemPath() {
            IImmutableSet<IProjectTree> nodes1;
            IImmutableSet<IProjectTree> nodes2;
            var filePath = @"C:\Temp";
            CreateTestNodeSetPair(filePath, out nodes1, out nodes2);

            var cmd = new CopyItemPathCommand(_interactiveWorkflowProvider);
            await InUI(() => CheckSingleNodeCommandStatusAsync(cmd, RPackageCommandId.icmdCopyItemPath, nodes1, nodes2));

            var contents = await InUI(() => Clipboard.GetText());
            contents.Should().Be("\"" + filePath + "\"");
        }

        [Test]
        [Category.Project]
        public void OpenContainingFolder() {
            IImmutableSet<IProjectTree> nodes1;
            IImmutableSet<IProjectTree> nodes2;
            var folder = @"C:\Temp";
            var file = "file.r";
            var filePath = Path.Combine(folder, file);

            CreateTestNodeSetPair(filePath, out nodes1, out nodes2);

            var ps = Substitute.For<IProcessServices>();
            ps.When(x => x.Start(Arg.Any<string>())).Do((c) => {
                c.Args()[0].Should().Be(folder);
            });

            var cmd = new OpenContainingFolderCommand(null, ps);
            CheckSingleNodeCommandStatus(cmd, RPackageCommandId.icmdOpenContainingFolder, nodes1, nodes2);
        }

        [Test]
        [Category.Project]
        public void OpenCommandPrompt() {
            IImmutableSet<IProjectTree> nodes1;
            IImmutableSet<IProjectTree> nodes2;
            var folder = @"C:\Temp";
            var file = "file.r";
            var filePath = Path.Combine(folder, file);

            CreateTestNodeSetPair(filePath, out nodes1, out nodes2);

            var ps = Substitute.For<IProcessServices>();
            ps.When(x => x.Start(Arg.Any<string>())).Do((c) => {
                c.Args()[0].Should().Be(folder);
            });

            var cmd = new OpenCommandPromptCommand(ps);
            CheckSingleNodeCommandStatus(cmd, RPackageCommandId.icmdOpenCmdPromptHere, nodes1, nodes2);
        }

        private void CreateTestNodeSetPair(string filePath, out IImmutableSet<IProjectTree> nodes1, out IImmutableSet<IProjectTree> nodes2) {
            var node1 = Substitute.For<IProjectTree>();
            var node2 = Substitute.For<IProjectTree>();
            node1.FilePath.Returns(filePath);
            nodes1 = ImmutableHashSet.Create(node1);
            nodes2 = ImmutableHashSet.Create(node1, node2);
        }

        private async Task CheckSingleNodeCommandStatusAsync(IAsyncCommandGroupHandler cmd, int id, IImmutableSet<IProjectTree> nodes1, IImmutableSet<IProjectTree> nodes2) {
            var csr = await cmd.GetCommandStatusAsync(nodes1, 0, false, string.Empty, CommandStatus.Enabled);
            csr.Should().Be(CommandStatusResult.Unhandled);

            csr = await cmd.GetCommandStatusAsync(nodes1, id, false, string.Empty, CommandStatus.Enabled);
            csr.Status.Should().Be(CommandStatus.Enabled | CommandStatus.Supported);

            csr = await cmd.GetCommandStatusAsync(nodes2, id, false, string.Empty, CommandStatus.Enabled);
            csr.Should().Be(CommandStatusResult.Unhandled);

            bool result = await cmd.TryHandleCommandAsync(nodes1, id, false, 0, IntPtr.Zero, IntPtr.Zero);
            result.Should().BeTrue();
        }

        private void CheckSingleNodeCommandStatus(ICommandGroupHandler cmd, int id, IImmutableSet<IProjectTree> nodes1, IImmutableSet<IProjectTree> nodes2) {
            var csr = cmd.GetCommandStatus(nodes1, 0, false, string.Empty, CommandStatus.Enabled);
            csr.Should().Be(CommandStatusResult.Unhandled);

            csr = cmd.GetCommandStatus(nodes1, id, false, string.Empty, CommandStatus.Enabled);
            csr.Status.Should().Be(CommandStatus.Enabled | CommandStatus.Supported);

            csr = cmd.GetCommandStatus(nodes2, id, false, string.Empty, CommandStatus.Enabled);
            csr.Should().Be(CommandStatusResult.Unhandled);

            bool result = cmd.TryHandleCommand(nodes1, id, false, 0, IntPtr.Zero, IntPtr.Zero);
            result.Should().BeTrue();
        }
    }
}

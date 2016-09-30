// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.Commands {
    [ExportCommandGroup("AD87578C-B324-44DC-A12A-B01A6ED5C6E3")]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal sealed class SendToRemoteCommand : SendFileCommandBase, IAsyncCommandGroupHandler {
        private readonly ConfiguredProject _configuredProject;
        private readonly IRInteractiveWorkflowProvider _interactiveWorkflowProvider;
        private readonly IApplicationShell _appShell;

        [ImportingConstructor]
        public SendToRemoteCommand(ConfiguredProject configuredProject, IRInteractiveWorkflowProvider interactiveWorkflowProvider, IApplicationShell appShell) :
            base(interactiveWorkflowProvider, appShell, new FileSystem()) {
            _configuredProject = configuredProject;
            _interactiveWorkflowProvider = interactiveWorkflowProvider;
            _appShell = appShell;
        }

        public Task<CommandStatusResult> GetCommandStatusAsync(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus progressiveStatus) {
            var session = _interactiveWorkflowProvider.GetOrCreate().RSession;
            if (commandId == RPackageCommandId.icmdSendToRemote && session.IsHostRunning && session.IsRemote) {
                return Task.FromResult(new CommandStatusResult(true, commandText, CommandStatus.Enabled | CommandStatus.Supported));
            }
            return Task.FromResult(CommandStatusResult.Unhandled);
        }


        public async Task<bool> TryHandleCommandAsync(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, long commandExecuteOptions, IntPtr variantArgIn, IntPtr variantArgOut) {
            _appShell.AssertIsOnMainThread();
            if (commandId != RPackageCommandId.icmdSendToRemote) {
                return false;
            }

            var properties = _configuredProject.Services.ExportProvider.GetExportedValue<ProjectProperties>();
            string projectDir = Path.GetDirectoryName(_configuredProject.UnconfiguredProject.FullPath);
            string projectName = properties.GetProjectName();
            string remotePath = await properties.GetRemoteProjectPathAsync();

            await SendToRemoteAsync(nodes.GetAllFilePaths(), projectDir, projectName, remotePath);

            return true;
        }
    }
}

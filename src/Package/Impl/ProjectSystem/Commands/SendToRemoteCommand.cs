// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.Extensions;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring;
using Microsoft.VisualStudio.R.Package.Commands;
#if VS14
using Microsoft.VisualStudio.ProjectSystem.Designers;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
#endif

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.Commands {
    [ExportCommandGroup("AD87578C-B324-44DC-A12A-B01A6ED5C6E3")]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal sealed class SendToRemoteCommand : IAsyncCommandGroupHandler {
        private readonly ConfiguredProject _configuredProject;
        private readonly IRInteractiveWorkflowProvider _interactiveWorkflowProvider;
        private readonly ICoreShell _coreShell;

        [ImportingConstructor]
        public SendToRemoteCommand(ConfiguredProject configuredProject, IRInteractiveWorkflowProvider interactiveWorkflowProvider, ICoreShell coreShell) {
            _configuredProject = configuredProject;
            _interactiveWorkflowProvider = interactiveWorkflowProvider;
            _coreShell = coreShell;
        }

        public Task<CommandStatusResult> GetCommandStatusAsync(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus progressiveStatus) {
            var session = _interactiveWorkflowProvider.GetOrCreate().RSession;
            if (commandId == RPackageCommandId.icmdSendToRemote && session.IsHostRunning && session.IsRemote) {
                return Task.FromResult(new CommandStatusResult(true, commandText, CommandStatus.Enabled | CommandStatus.Supported));
            }
            return Task.FromResult(CommandStatusResult.Unhandled);
        }


        public async Task<bool> TryHandleCommandAsync(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, long commandExecuteOptions, IntPtr variantArgIn, IntPtr variantArgOut) {
            _coreShell.AssertIsOnMainThread();
            if (commandId != RPackageCommandId.icmdSendToRemote) {
                return false;
            }

            var workflow = _interactiveWorkflowProvider.GetOrCreate();
            try {
                var session = workflow.RSession;
                var properties = _configuredProject.Services.ExportProvider.GetExportedValue<ProjectProperties>();

                string projectDir = Path.GetDirectoryName(_configuredProject.UnconfiguredProject.FullPath);
                string projectName = properties.GetProjectName();
                string remotePath = await properties.GetRemoteProjectPathAsync();
                var files = nodes.GetAllFilePaths();

                workflow.ActiveWindow.InteractiveWindow.WriteLine(Resources.Info_CompressingFiles);
                IFileSystem fs = new FileSystem();
                string compressedFilePath = fs.CompressFiles(files, projectDir, new Progress<string>((p) => {
                    workflow.ActiveWindow.InteractiveWindow.WriteLine(string.Format(Resources.Info_LocalFilePath, p));
                    string dest = p.MakeRelativePath(projectDir).ProjectRelativePathToRemoteProjectPath(remotePath, projectName);
                    workflow.ActiveWindow.InteractiveWindow.WriteLine(string.Format(Resources.Info_RemoteFilePath, dest));

                }), CancellationToken.None);

                using (var fts = new DataTransferSession(session, fs)) {
                    workflow.ActiveWindow.InteractiveWindow.WriteLine(Resources.Info_TransferingFiles);
                    var remoteFile = await fts.SendFileAsync(compressedFilePath);
                    await session.EvaluateAsync<string>($"rtvs:::save_to_project_folder({remoteFile.Id}, {projectName.ToRStringLiteral()}, '{remotePath.ToRPath()}')", REvaluationKind.Normal);
                    workflow.ActiveWindow.InteractiveWindow.WriteLine(Resources.Info_TransferingFilesDone);
                }
            } catch (RHostDisconnectedException) {
                workflow.ActiveWindow.InteractiveWindow.WriteErrorLine(Resources.Error_CannotTransferNoRSession);
            }

            return true;
        }
    }
}

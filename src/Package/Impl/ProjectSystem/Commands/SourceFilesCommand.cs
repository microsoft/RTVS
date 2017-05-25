// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.ContentTypes;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Host;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring;
using Microsoft.VisualStudio.R.Package.Commands;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.Commands {
    [ExportCommandGroup("AD87578C-B324-44DC-A12A-B01A6ED5C6E3")]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal sealed class SourceFilesCommand : SendFileCommandBase, IAsyncCommandGroupHandler {
        private readonly ConfiguredProject _configuredProject;
        private IRInteractiveWorkflowVisualProvider _interactiveWorkflowProvider;
        private readonly ICoreShell _shell;

        [ImportingConstructor]
        public SourceFilesCommand(ConfiguredProject configuredProject, IRInteractiveWorkflowVisualProvider interactiveWorkflowProvider, ICoreShell shell) :
            base(interactiveWorkflowProvider, shell.UI(), shell.FileSystem()) {
            _configuredProject = configuredProject;
            _interactiveWorkflowProvider = interactiveWorkflowProvider;
            _shell = shell;
        }

        public Task<CommandStatusResult> GetCommandStatusAsync(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, string commandText, CommandStatus progressiveStatus) {
            if ((commandId == RPackageCommandId.icmdSourceSelectedFiles || commandId == RPackageCommandId.icmdSourceSelectedFilesWithEcho) && nodes.GetSelectedNodesPaths().Count() > 0) {
                foreach (var n in nodes) {
                    if (n.IsFolder || !Path.GetExtension(n.FilePath).EqualsIgnoreCase(".r")) { 
                        return Task.FromResult(CommandStatusResult.Unhandled);
                    }
                }
                return Task.FromResult(new CommandStatusResult(true, commandText, CommandStatus.Enabled | CommandStatus.Supported));
            }
            return Task.FromResult(CommandStatusResult.Unhandled);
        }

        public async Task<bool> TryHandleCommandAsync(IImmutableSet<IProjectTree> nodes, long commandId, bool focused, long commandExecuteOptions, IntPtr variantArgIn, IntPtr variantArgOut) {
            if (commandId == RPackageCommandId.icmdSourceSelectedFiles || commandId == RPackageCommandId.icmdSourceSelectedFilesWithEcho) {
                bool echo = commandId == RPackageCommandId.icmdSourceSelectedFilesWithEcho;
                
                IFileSystem fs = _shell.FileSystem();
                IEnumerable<string> rFiles = Enumerable.Empty<string>();

                var workflow = _interactiveWorkflowProvider.GetOrCreate();
                try {
                    var session = workflow.RSession;
                    if (session.IsRemote) {
                        var files = nodes.GetSelectedNodesPaths().Where(x =>
                                   Path.GetExtension(x).EqualsIgnoreCase(RContentTypeDefinition.FileExtension) &&
                                   fs.FileExists(x));

                        var properties = _configuredProject.Services.ExportProvider.GetExportedValue<ProjectProperties>();
                        string projectDir = Path.GetDirectoryName(_configuredProject.UnconfiguredProject.FullPath);
                        string projectName = properties.GetProjectName();
                        string remotePath = await properties.GetRemoteProjectPathAsync();

                        await SendToRemoteAsync(files, projectDir, projectName, remotePath);

                        rFiles = files.Select(p => p.MakeRelativePath(projectDir).ProjectRelativePathToRemoteProjectPath(remotePath, projectName));
                    } else {
                        rFiles = nodes.GetSelectedNodesPaths().Where(x =>
                                   Path.GetExtension(x).EqualsIgnoreCase(RContentTypeDefinition.FileExtension) &&
                                   fs.FileExists(x));
                    }

                    workflow.Operations.SourceFiles(rFiles, echo);
                } catch (IOException ex) {
                    _shell.ShowErrorMessage(string.Format(CultureInfo.InvariantCulture, Resources.Error_CannotTransferFile, ex.Message));
                } 
                catch (RHostDisconnectedException) {
                    workflow.ActiveWindow.InteractiveWindow.WriteErrorLine(Resources.Error_CannotTransferNoRSession);
                }
                return true;
            }
            return false;
        }
    }
}

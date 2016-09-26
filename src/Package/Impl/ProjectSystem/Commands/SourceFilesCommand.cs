// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.IO;
using Microsoft.R.Components.ContentTypes;
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
#if VS15
using Microsoft.VisualStudio.ProjectSystem;
#endif

namespace Microsoft.VisualStudio.R.Package.ProjectSystem.Commands {
    [ExportCommandGroup("AD87578C-B324-44DC-A12A-B01A6ED5C6E3")]
    [AppliesTo(ProjectConstants.RtvsProjectCapability)]
    internal sealed class SourceFilesCommand : IAsyncCommandGroupHandler {
        private readonly ConfiguredProject _configuredProject;
        private IRInteractiveWorkflowProvider _interactiveWorkflowProvider;

        [ImportingConstructor]
        public SourceFilesCommand(ConfiguredProject configuredProject, IRInteractiveWorkflowProvider interactiveWorkflowProvider)  {
            _configuredProject = configuredProject;
            _interactiveWorkflowProvider = interactiveWorkflowProvider;
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
                IEnumerable<string> rFiles = Enumerable.Empty<string>();

                IFileSystem fs = new FileSystem();
                var workflow = _interactiveWorkflowProvider.GetOrCreate();
                try {
                    var session = workflow.RSession;
                    if (session.IsRemote) {
                        var properties = _configuredProject.Services.ExportProvider.GetExportedValue<ProjectProperties>();
                        
                        string projectDir = Path.GetDirectoryName(_configuredProject.UnconfiguredProject.FullPath);
                        string projectName = properties.GetProjectName();
                        string remotePath = await properties.GetRemoteProjectPathAsync();

                        var files = nodes.GetSelectedNodesPaths().Where(x =>
                                   Path.GetExtension(x).EqualsIgnoreCase(RContentTypeDefinition.FileExtension) &&
                                   fs.FileExists(x));
                        workflow.ActiveWindow.InteractiveWindow.WriteLine(Resources.Info_CompressingFiles);
                        
                        string compressedFilePath = fs.CompressFiles(files, projectDir, new Progress<string>((p) => {
                            workflow.ActiveWindow.InteractiveWindow.WriteLine(string.Format(Resources.Info_LocalFilePath, p));
                            string dest = p.MakeRelativePath(projectDir).ProjectRelativePathToRemoteProjectPath(remotePath, projectName);
                            workflow.ActiveWindow.InteractiveWindow.WriteLine(string.Format(Resources.Info_RemoteDestination, dest));

                        }), CancellationToken.None);

                        using (var fts = new DataTransferSession(session, fs)) {
                            workflow.ActiveWindow.InteractiveWindow.WriteLine(Resources.Info_TransferingFiles);
                            var remoteFile = await fts.SendFileAsync(compressedFilePath);
                            await session.EvaluateAsync<string>($"rtvs:::save_to_project_folder({remoteFile.Id}, {projectName.ToRStringLiteral()}, '{remotePath.ToRPath()}')", REvaluationKind.Normal);
                            workflow.ActiveWindow.InteractiveWindow.WriteLine(Resources.Info_TransferingFilesDone);
                        }

                        rFiles = files.Select(p => p.MakeRelativePath(projectDir).ProjectRelativePathToRemoteProjectPath(remotePath, projectName));
                    } else {
                        rFiles = nodes.GetSelectedNodesPaths().Where(x =>
                                   Path.GetExtension(x).EqualsIgnoreCase(RContentTypeDefinition.FileExtension) &&
                                   fs.FileExists(x));
                    }

                    workflow.Operations.SourceFiles(rFiles, echo);
                    return true;
                } catch (RHostDisconnectedException) {
                    workflow.ActiveWindow.InteractiveWindow.WriteErrorLine(Resources.Error_CannotTransferNoRSession);
                    return false;
                }
            }
            return false;
        }
    }
}

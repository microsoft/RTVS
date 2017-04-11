// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Components.InteractiveWorkflow;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Workspace {
    internal sealed class LoadWorkspaceCommand : PackageCommand {
        private readonly ICoreShell _shell;
        private readonly IRInteractiveWorkflowVisual _interactiveWorkflow;
        private readonly IRSession _rSession;
        private readonly IProjectServiceAccessor _projectServiceAccessor;

        public LoadWorkspaceCommand(ICoreShell shell, IRInteractiveWorkflowVisual interactiveWorkflow, IProjectServiceAccessor projectServiceAccessor) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdLoadWorkspace) {
            _shell = shell;
            _interactiveWorkflow = interactiveWorkflow;
            _rSession = interactiveWorkflow.RSession;
            _projectServiceAccessor = projectServiceAccessor;
        }

        protected override void SetStatus() {
            var window = _interactiveWorkflow.ActiveWindow;
            if (window != null && window.Container.IsOnScreen) {
                Visible = true;
                Enabled = _rSession.IsHostRunning && !_rSession.IsRemote;
            } else {
                Visible = false;
            }
        }

        protected override void Handle() {
            var projectService = _projectServiceAccessor.GetProjectService();
            var lastLoadedProject = projectService.LoadedUnconfiguredProjects.LastOrDefault();

            var initialPath = lastLoadedProject != null ? lastLoadedProject.GetProjectDirectory() : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var file = _shell.FileDialog().ShowOpenFileDialog(Resources.WorkspaceFileFilter, initialPath, Resources.LoadWorkspaceTitle);
            if (file == null) {
                return;
            }

            LoadWorkspace(_rSession, file).DoNotWait();
        }

        private async Task LoadWorkspace(IRSession session, string file) {
            try {
                await session.LoadWorkspaceAsync(file);
            } catch (RException ex) {
                var message = Resources.LoadWorkspaceFailedMessageFormat.FormatCurrent(file, ex.Message);
                await _shell.ShowErrorMessageAsync(message);
            } catch (OperationCanceledException) {
            }
        }
    }
}

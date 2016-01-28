using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Shell;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Workspace {
    internal sealed class SaveWorkspaceCommand : PackageCommand {
        private readonly IRSession _rSession;
        private readonly IProjectServiceAccessor _projectServiceAccessor;

        public SaveWorkspaceCommand(IRSessionProvider rSessionProvider, IProjectServiceAccessor projectServiceAccessor) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSaveWorkspace) {
            _rSession = rSessionProvider.GetInteractiveWindowRSession();
            _projectServiceAccessor = projectServiceAccessor;
        }

        internal override void SetStatus() {
            if (ReplWindow.Current.IsActive) {
                Visible = true;
                Enabled = _rSession.IsHostRunning;
            } else {
                Visible = false;
            }
        }

        internal override void Handle() {
            var projectService = _projectServiceAccessor.GetProjectService();
            var lastLoadedProject = projectService.LoadedUnconfiguredProjects.LastOrDefault();

            var initialPath = lastLoadedProject != null ? lastLoadedProject.GetProjectDirectory() : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var file = VsAppShell.Current.BrowseForFileSave(IntPtr.Zero, Resources.WorkspaceFileFilter, initialPath, Resources.SaveWorkspaceAsTitle);
            if (file == null) {
                return;
            }

            SaveWorkspace(file).DoNotWait();
        }

        private async Task SaveWorkspace(string file) {
            REvaluationResult result;
            using (var evaluation = await _rSession.BeginEvaluationAsync()) {
                result = await evaluation.SaveWorkspace(file);
            }

            if (result.Error != null) {
                var message = string.Format(CultureInfo.CurrentCulture, Resources.SaveWorkspaceFailedMessageFormat, file, result.Error);
                VsAppShell.Current.ShowErrorMessage(message);
            }
        }
    }
}

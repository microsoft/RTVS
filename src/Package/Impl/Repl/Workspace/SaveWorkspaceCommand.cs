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
        private readonly IRSessionProvider _rSessionProvider;
        private readonly IProjectServiceAccessor _projectServiceAccessor;

        public SaveWorkspaceCommand(IRSessionProvider rSessionProvider, IProjectServiceAccessor projectServiceAccessor) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdSaveWorkspace) {
            _rSessionProvider = rSessionProvider;
            _projectServiceAccessor = projectServiceAccessor;
        }

        protected override void SetStatus() {
            if (ReplWindow.Current.IsActive) {
                Visible = true;
                Enabled = (_rSessionProvider.Current != null);
            } else {
                Visible = false;
            }
        }

        protected override void Handle() {
            var session = _rSessionProvider.Current;
            if (session == null) {
                return;
            }

            var projectService = _projectServiceAccessor.GetProjectService();
            var lastLoadedProject = projectService.LoadedUnconfiguredProjects.LastOrDefault();

            var initialPath = lastLoadedProject != null ? lastLoadedProject.GetProjectDirectory() : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var file = VsAppShell.Current.BrowseForFileSave(IntPtr.Zero, Resources.WorkspaceFileFilter, initialPath, Resources.SaveWorkspaceAsTitle);
            if (file == null) {
                return;
            }

            SaveWorkspace(session, file).DoNotWait();
        }

        private async Task SaveWorkspace(IRSession session, string file) {
            REvaluationResult result;
            using (var evaluation = await session.BeginEvaluationAsync()) {
                result = await evaluation.SaveWorkspace(file);
            }

            if (result.Error != null) {
                var message = string.Format(CultureInfo.CurrentCulture, Resources.SaveWorkspaceFailedMessageFormat, file, result.Error);
                VsAppShell.Current.ShowErrorMessage(message);
            }
        }
    }
}

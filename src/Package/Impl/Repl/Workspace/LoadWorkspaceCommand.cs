using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Common.Core;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project;
using Microsoft.VisualStudio.R.Package.Commands;
using Microsoft.VisualStudio.R.Package.Repl.Session;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.Repl.Workspace {
    internal sealed class LoadWorkspaceCommand : PackageCommand {
        private readonly IRSessionProvider _rSessionProvider;
        private readonly IProjectServiceAccessor _projectServiceAccessor;

        public LoadWorkspaceCommand(IRSessionProvider rSessionProvider, IProjectServiceAccessor projectServiceAccessor) :
            base(RGuidList.RCmdSetGuid, RPackageCommandId.icmdLoadWorkspace) {
            _rSessionProvider = rSessionProvider;
            _projectServiceAccessor = projectServiceAccessor;
        }

        protected override void SetStatus() {
            Enabled = _rSessionProvider.Current != null;
        }

        protected override void Handle() {
            var session = _rSessionProvider.Current;
            if (session == null) {
                return;
            }

            var projectService = _projectServiceAccessor.GetProjectService();
            var lastLoadedProject = projectService.LoadedUnconfiguredProjects.LastOrDefault();

            var initialPath = lastLoadedProject != null ? lastLoadedProject.GetProjectDirectory() : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var file = EditorShell.Current.BrowseForFileOpen(IntPtr.Zero, Resources.WorkspaceFileFilter, initialPath, Resources.LoadWorkspaceTitle);
            if (file == null) {
                return;
            }

            LoadWorkspace(session, file).DoNotWait();
        }

        private async Task LoadWorkspace(IRSession session, string file) {
            REvaluationResult result;
            using (var evaluation = await session.BeginEvaluationAsync()) {
                result = await evaluation.LoadWorkspace(file);
            }

            if (result.Error != null) {
                var message = string.Format(CultureInfo.CurrentCulture, Resources.LoadWorkspaceFailedMessageFormat, file, result.Error);
                EditorShell.Current.ShowErrorMessage(message);    
            }
        }
    }
}

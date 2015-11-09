using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Common.Core.Enums;
using Microsoft.Common.Core.IO;
using Microsoft.Languages.Editor.Shell;
using Microsoft.R.Host.Client;
using Microsoft.R.Support.Settings;
using Microsoft.R.Support.Settings.Definitions;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Repl.Session;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    [AppliesTo("RTools")]
    internal sealed class RProjectLoadHooks {
        private const string DefaultRDataName = ".RData";

        [Export(typeof(IFileSystemMirroringProjectTemporaryItems))]
        private FileSystemMirroringProject Project { get; }

        private readonly MsBuildFileSystemWatcher _fileWatcher;
        private readonly string _projectDirectory;
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRToolsSettings _toolsSettings;
        private readonly IFileSystem _fileSystem;
        private readonly UnconfiguredProject _unconfiguredProject;

        [ImportingConstructor]
        public RProjectLoadHooks(UnconfiguredProject unconfiguredProject, IProjectLockService projectLockService, IRSessionProvider sessionProvider, IRToolsSettings toolsSettings, IFileSystem fileSystem) {
            _unconfiguredProject = unconfiguredProject;
            _sessionProvider = sessionProvider;
            _toolsSettings = toolsSettings;
            _fileSystem = fileSystem;
            _projectDirectory = unconfiguredProject.GetProjectDirectory();

            unconfiguredProject.ProjectUnloading += ProjectUnloading;
            _fileWatcher = new MsBuildFileSystemWatcher(_projectDirectory, "*", 25, fileSystem, new RMsBuildFileSystemFilter());
            Project = new FileSystemMirroringProject(unconfiguredProject, projectLockService, _fileWatcher);
        }

        [AppliesTo("RTools")]
        [UnconfiguredProjectAutoLoad2(completeBy: UnconfiguredProjectLoadCheckpoint.CapabilitiesEstablished)]
        public async Task InitializeProjectFromDiskAsync() {
            await Project.CreateInMemoryImport();
            _fileWatcher.Start();

            // Force REPL window up
            await ReplWindow.EnsureReplWindow();

            var currentSession = _sessionProvider.Current;
            if (currentSession != null) {
                var rdataPath = Path.Combine(_projectDirectory, DefaultRDataName);
                bool loadDefaultWorkspace = _fileSystem.FileExists(rdataPath) && GetLoadDefaultWorkspace(rdataPath);

                using (var evaluation = await _sessionProvider.Current.BeginEvaluationAsync()) {
                    if (loadDefaultWorkspace) {
                        await evaluation.LoadWorkspace(rdataPath);
                    }
                    await evaluation.SetWorkingDirectory(_projectDirectory);
                    RToolsSettings.Current.WorkingDirectory = _projectDirectory;
                }
            }
        }

        private async Task ProjectUnloading(object sender, EventArgs args) {
            _unconfiguredProject.ProjectUnloading -= ProjectUnloading;
            var currentSession = _sessionProvider.Current;
            if (currentSession != null) {
                var rdataPath = Path.Combine(_projectDirectory, DefaultRDataName);
                var saveDefaultWorkspace = GetSaveDefaultWorkspace(rdataPath);

                using (var evaluation = await currentSession.BeginEvaluationAsync()) {
                    if (saveDefaultWorkspace) {
                        await evaluation.SaveWorkspace(rdataPath);
                    }
                    await evaluation.SetDefaultWorkingDirectory();
                }
            }
        }

        private bool GetLoadDefaultWorkspace(string rdataPath) {
            switch (_toolsSettings.LoadRDataOnProjectLoad) {
                case YesNoAsk.Yes:
                    return true;
                case YesNoAsk.Ask:
                    return true;
                //TODO: Find out when it is safe to show message box during project loading
                //return EditorShell.Current.ShowYesNoMessage(
                //    string.Format(CultureInfo.CurrentCulture, Resources.LoadWorkspaceIntoGlobalEnvironment, rdataPath),
                //    Resources.LoadWorkspaceTitle);
                case YesNoAsk.No:
                default:
                    return false;
            }
        }

        private bool GetSaveDefaultWorkspace(string rdataPath) {
            switch (_toolsSettings.SaveRDataOnProjectUnload) {
                case YesNoAsk.Yes:
                    return true;
                case YesNoAsk.Ask:
                    return EditorShell.Current.ShowYesNoMessage(
                        string.Format(CultureInfo.CurrentCulture, Resources.SaveWorkspaceOnProjectUnload, rdataPath),
                        Resources.SaveWorkspaceOnProjectUnloadTitle);
                case YesNoAsk.No:
                default:
                    return false;
            }
        }
    }
}
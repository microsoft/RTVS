using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Common.Core.Enums;
using Microsoft.Common.Core.IO;
using Microsoft.Common.Core.Shell;
using Microsoft.R.Host.Client;
using Microsoft.R.Host.Client.Session;
using Microsoft.R.Support.Settings.Definitions;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.R.Package.History;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Shell;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    [AppliesTo("RTools")]
    internal sealed class RProjectLoadHooks {
        private const string DefaultRDataName = ".RData";
        private const string DefaultRHistoryName = ".RHistory";

        [Export(typeof(IFileSystemMirroringProjectTemporaryItems))]
        private FileSystemMirroringProject Project { get; }

        private readonly MsBuildFileSystemWatcher _fileWatcher;
        private readonly string _projectDirectory;
        private readonly IRSessionProvider _sessionProvider;
        private readonly IRToolsSettings _toolsSettings;
        private readonly IFileSystem _fileSystem;
        private readonly IThreadHandling _threadHandling;
        private readonly UnconfiguredProject _unconfiguredProject;

        [ImportingConstructor]
        public RProjectLoadHooks(UnconfiguredProject unconfiguredProject, IProjectLockService projectLockService, IRSessionProvider sessionProvider, IRToolsSettings toolsSettings, IFileSystem fileSystem, IThreadHandling threadHandling) {
            _unconfiguredProject = unconfiguredProject;
            _sessionProvider = sessionProvider;
            _toolsSettings = toolsSettings;
            _fileSystem = fileSystem;
            _threadHandling = threadHandling;
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
            if (currentSession == null) {
                return;
            }

            var rdataPath = Path.Combine(_projectDirectory, DefaultRDataName);
            bool loadDefaultWorkspace = _fileSystem.FileExists(rdataPath) && await GetLoadDefaultWorkspace(rdataPath);

            using (var evaluation = await _sessionProvider.Current.BeginEvaluationAsync()) {
                if (loadDefaultWorkspace) {
                    await evaluation.LoadWorkspace(rdataPath);
                }

                await evaluation.SetWorkingDirectory(_projectDirectory);
            }

            await _threadHandling.SwitchToUIThread();

            _toolsSettings.WorkingDirectory = _projectDirectory;
            var history = GetRHistory();
            if (history != null) {
                history.TryLoadFromFile(Path.Combine(_projectDirectory, DefaultRHistoryName));
            }
        }

        private async Task ProjectUnloading(object sender, EventArgs args) {
            _unconfiguredProject.ProjectUnloading -= ProjectUnloading;
            var currentSession = _sessionProvider.Current;
            if (currentSession == null) {
                return;
            }

            var rdataPath = Path.Combine(_projectDirectory, DefaultRDataName);
            var saveDefaultWorkspace = await GetSaveDefaultWorkspace(rdataPath);

            using (var evaluation = await currentSession.BeginEvaluationAsync()) {
                if (saveDefaultWorkspace) {
                    await evaluation.SaveWorkspace(rdataPath);
                }
                await evaluation.SetDefaultWorkingDirectory();
            }

            if (saveDefaultWorkspace || _toolsSettings.AlwaysSaveHistory) {
                await _threadHandling.SwitchToUIThread();
                var history = GetRHistory();
                if (history != null) {
                    history.TrySaveToFile(Path.Combine(_projectDirectory, DefaultRHistoryName));
                }
            }
        }

        private async Task<bool> GetLoadDefaultWorkspace(string rdataPath) {
            switch (_toolsSettings.LoadRDataOnProjectLoad) {
                case YesNoAsk.Yes:
                    return true;
                case YesNoAsk.Ask:
                    await _threadHandling.SwitchToUIThread();
                    return VsAppShell.Current.ShowMessage(
                        string.Format(CultureInfo.CurrentCulture, Resources.LoadWorkspaceIntoGlobalEnvironment, rdataPath),
                        MessageButtons.YesNo) == MessageButtons.Yes;
                default:
                    return false;
            }
        }

        private async Task<bool> GetSaveDefaultWorkspace(string rdataPath) {
            switch (_toolsSettings.SaveRDataOnProjectUnload) {
                case YesNoAsk.Yes:
                    return true;
                case YesNoAsk.Ask:
                    await _threadHandling.SwitchToUIThread();
                    return VsAppShell.Current.ShowMessage(
                        string.Format(CultureInfo.CurrentCulture, Resources.SaveWorkspaceOnProjectUnload, rdataPath),
                        MessageButtons.YesNo) == MessageButtons.Yes;
                default:
                    return false;
            }
        }

        private static IRHistory GetRHistory() {
            return GetRInteractiveEvaluator()?.History;
        }

        private static RInteractiveEvaluator GetRInteractiveEvaluator() {
            return ReplWindow.Current.GetInteractiveWindow()?.InteractiveWindow.Evaluator as RInteractiveEvaluator;
        }
    }
}
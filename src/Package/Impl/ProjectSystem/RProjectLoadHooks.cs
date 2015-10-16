using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.Common.Core.IO;
using Microsoft.R.Host.Client;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.R.Package.Repl;
using Microsoft.VisualStudio.R.Package.Repl.Session;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem
{
    [AppliesTo("RTools")]
    internal sealed class RProjectLoadHooks
    {
        [Export(typeof(IFileSystemMirroringProjectTemporaryItems))]
        private FileSystemMirroringProject Project { get; }

        private readonly MsBuildFileSystemWatcher _fileWatcher;
        private readonly string _projectDirectory;
        private readonly IRSessionProvider _sessionProvider;
        private readonly UnconfiguredProject _unconfiguredProject;

        [ImportingConstructor]
        public RProjectLoadHooks(UnconfiguredProject unconfiguredProject, IProjectLockService projectLockService, IRSessionProvider sessionProvider, IFileSystem fileSystem)
        {
            _unconfiguredProject = unconfiguredProject;
            _sessionProvider = sessionProvider;
            _projectDirectory = unconfiguredProject.GetProjectDirectory();
            unconfiguredProject.ProjectUnloading += ProjectUnloading;
            _fileWatcher = new MsBuildFileSystemWatcher(_projectDirectory, "*", 25, fileSystem, new RMsBuildFileSystemFilter());
            Project = new FileSystemMirroringProject(unconfiguredProject, projectLockService, _fileWatcher);
        }

        [AppliesTo("RTools")]
        [UnconfiguredProjectAutoLoad2(completeBy: UnconfiguredProjectLoadCheckpoint.CapabilitiesEstablished)]
        public async Task InitializeProjectFromDiskAsync()
        {
            await Project.CreateInMemoryImport();
            _fileWatcher.Start();

            // Force REPL window up
            await ReplWindow.EnsureReplWindow();

            var currentSession = _sessionProvider.Current;
            if (currentSession != null) {
                using (var evaluation = await _sessionProvider.Current.BeginEvaluationAsync()) {
                    await evaluation.SetWorkingDirectory(_projectDirectory);
                }
            }
        }

        private async Task ProjectUnloading(object sender, EventArgs args)
        {
            _unconfiguredProject.ProjectUnloading -= ProjectUnloading;
            var currentSession = _sessionProvider.Current;
            if (currentSession != null) {
                using (var evaluation = await currentSession.BeginEvaluationAsync()) {
                    await evaluation.SetDefaultWorkingDirectory();
                }
            }
        }
    }
}
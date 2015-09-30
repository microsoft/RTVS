using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO.FileSystem;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project;
using Microsoft.VisualStudio.ProjectSystem.Utilities;
using Microsoft.VisualStudio.R.Package.Repl;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem
{
    [AppliesTo("RTools")]
    internal sealed class RProjectLoadHooks
    {
        [Export(typeof(IFileSystemMirroringProjectTemporaryItems))]
        private FileSystemMirroringProject Project { get; }

        private readonly MsBuildFileSystemWatcher _fileWatcher;

        [ImportingConstructor]
        public RProjectLoadHooks(UnconfiguredProject unconfiguredProject, IProjectLockService projectLockService)
        {
            _fileWatcher = new MsBuildFileSystemWatcher(unconfiguredProject.GetProjectDirectory(), "*", 25, new FileSystemProxy(), new RMsBuildFileSystemFilter());
            Project = new FileSystemMirroringProject(unconfiguredProject, projectLockService, _fileWatcher);

            // Force REPL window up
            ReplWindow.EnsureReplWindow();
        }

        [AppliesTo("RTools")]
        [UnconfiguredProjectAutoLoad2(completeBy: UnconfiguredProjectLoadCheckpoint.CapabilitiesEstablished)]
        public async Task InitializeProjectFromDiskAsync()
        {
            await Project.CreateInMemoryImport();
            _fileWatcher.Start();
        }
    }
}
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.IO;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem
{
	[AppliesTo("RTools")]
	internal sealed class RProjectLoadHooks
	{
		private readonly FileSystemMirroringProject _project;
		private readonly MsBuildFileSystemWatcher _fileWatcher;

		[ImportingConstructor]
		public RProjectLoadHooks(UnconfiguredProject unconfiguredProject, IProjectLockService projectLockService)
		{
			_fileWatcher = new MsBuildFileSystemWatcher(unconfiguredProject.GetProjectDirectory(), "*", 25, new RMsBuildFileSystemFilter());
			_project = new FileSystemMirroringProject(unconfiguredProject, projectLockService, _fileWatcher);
		}

		[AppliesTo("RTools")]
		[UnconfiguredProjectAutoLoad2(completeBy: UnconfiguredProjectLoadCheckpoint.CapabilitiesEstablished)]
		public async Task InitializeProjectFromDiskAsync()
		{
			await _project.CreateInMemoryImport();
			_fileWatcher.Start();
		}
	}
}
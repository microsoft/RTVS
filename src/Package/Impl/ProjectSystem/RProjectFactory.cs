using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem
{
    [Guid(RGuidList.ProjectFileGeneratorGuidString)]
    internal sealed class RProjectFileGenerator : FileSystemMirroringProjectFileGenerator
    {
        public RProjectFileGenerator() 
			: base(RGuidList.CpsProjectFactoryGuid, "Visual Studio Tools for Language R", new [] { @"RTVS\Rules\rtvs.rules.props" })
        {
        }
    }

	[AppliesTo("RTools")]
	internal sealed class RProjectLoadHooks
	{
		private FileSystemMirroringProjectLoader _loader;

		[ImportingConstructor]
		public RProjectLoadHooks(UnconfiguredProject unconfiguredProject, IProjectLockService projectLockService, IThreadHandling threadHandling)
		{
			_loader = new FileSystemMirroringProjectLoader(unconfiguredProject, projectLockService, threadHandling);
        }

		[AppliesTo("RTools")]
		[UnconfiguredProjectAutoLoad2(completeBy: UnconfiguredProjectLoadCheckpoint.CapabilitiesEstablished)]
		public Task InitializeProjectFromDiskAsync()
		{
			return _loader.InitializeProjectFromDiskAsync();
		}
	}
}

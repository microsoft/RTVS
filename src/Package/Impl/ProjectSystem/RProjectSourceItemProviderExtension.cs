using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project;
using Microsoft.VisualStudio.ProjectSystem.Items;
using Microsoft.VisualStudio.ProjectSystem.Utilities;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem
{
	[Export(typeof(IProjectSourceItemProviderExtension))]
	[Export(typeof(IProjectFolderItemProviderExtension))]
	[AppliesTo("RTools")]
	internal sealed class RProjectSourceItemProviderExtension : FileSystemMirroringProjectSourceItemProviderExtensionBase
	{
		[ImportingConstructor]
		public RProjectSourceItemProviderExtension(UnconfiguredProject unconfiguredProject, ConfiguredProject configuredProject, IProjectLockService projectLockService, IFileSystemMirroringProjectTemporaryItems temporaryItems) 
			: base(unconfiguredProject, configuredProject, projectLockService, temporaryItems)
		{
		}
	}
}
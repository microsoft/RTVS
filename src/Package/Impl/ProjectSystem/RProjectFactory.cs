using System.Runtime.InteropServices;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem
{
    [Guid(GuidList.ProjectFileGeneratorGuidString)]
    internal sealed class RProjectFileGenerator : FileSystemMirroringProjectFileGenerator
    {
        public RProjectFileGenerator() 
			: base(GuidList.CpsProjectFactoryGuid, "Visual Studio Tools for Language R" , ".rxproj", new [] { @"RTVS\Rules\rtvs.rules.props" })
        {
        }
    }
}

using System.Runtime.InteropServices;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem
{
    [Guid(RGuidList.ProjectFileGeneratorGuidString)]
    internal sealed class RProjectFileGenerator : FileSystemMirroringProjectFileGenerator
    {
        public RProjectFileGenerator() 
			: base(RGuidList.CpsProjectFactoryGuid, "Visual Studio Tools for Language R" , ".rxproj", new [] { @"RTVS\Rules\rtvs.rules.props" })
        {
        }
    }
}

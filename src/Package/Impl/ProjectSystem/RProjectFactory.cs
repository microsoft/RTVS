using System.Runtime.InteropServices;
using Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.Project;
using Microsoft.VisualStudio.R.Packages.R;

namespace Microsoft.VisualStudio.R.Package.ProjectSystem {
    [Guid(RGuidList.ProjectFileGeneratorGuidString)]
    internal sealed class RProjectFileGenerator : FileSystemMirroringProjectFileGenerator {
        public RProjectFileGenerator()
            : base(RGuidList.CpsProjectFactoryGuid, null, ".rxproj", new[] { @"RTVS\Rules\rtvs.rules.props" }) {
        }
    }
}

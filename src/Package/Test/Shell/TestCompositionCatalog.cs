using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Editor.Test.Shell;

namespace Microsoft.VisualStudio.R.Package.Test.Shell {
    [ExcludeFromCodeCoverage]
    public sealed class TestCompositionCatalog : EditorTestCompositionCatalog {
        private static string[] _assemblies = new string[] {
            "Microsoft.VisualStudio.Shell.Mocks.dll",
            "Microsoft.VisualStudio.R.Package.dll",
            "Microsoft.VisualStudio.R.Package.Test.dll",
            "Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.dll",
        };
        public static new TestCompositionCatalog Current { get; } = new TestCompositionCatalog();
        public TestCompositionCatalog() : 
            base(_assemblies) {
        }
    }
}

using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Shell;
using Microsoft.Languages.Editor.Test.Shell;

namespace Microsoft.VisualStudio.R.Package.Test.Shell {
    [ExcludeFromCodeCoverage]
    public sealed class VsTestCompositionCatalog {
        private static readonly string[] _assemblies = {
            "Microsoft.VisualStudio.Shell.Mocks.dll",
            "Microsoft.VisualStudio.R.Package.dll",
            "Microsoft.VisualStudio.R.Package.Test.dll",
            "Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.dll",
        };

        public static ICompositionCatalog Current { get; } = new EditorTestCompositionCatalog(_assemblies);
    }
}

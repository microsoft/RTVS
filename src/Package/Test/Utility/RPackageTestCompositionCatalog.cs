using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Editor.Mocks;

namespace Microsoft.VisualStudio.R.Package.Test.Utility {
    [ExcludeFromCodeCoverage]
    public sealed class RPackageTestCompositionCatalog : TestCompositionCatalog {
        private static RPackageTestCompositionCatalog _instance;

        public static ITestCompositionCatalog Current {
            get {
                if (_instance == null) {
                    _instance = new RPackageTestCompositionCatalog();
                }
                return _instance;
            }
        }

        private static string[] _rPackageAssemblies = new string[] {
            "Microsoft.Markdown.Editor.dll",
            "Microsoft.Languages.Editor.dll",
            "Microsoft.R.Editor.dll",
            "Microsoft.R.Support.dll",
            "Microsoft.R.Common.Core.dll",
            "Microsoft.R.Host.Client.dll",
            "Microsoft.VisualStudio.Shell.Mocks.dll",
            "Microsoft.VisualStudio.R.Package.dll",
            "Microsoft.VisualStudio.R.Package.Test.dll",
            "Microsoft.VisualStudio.ProjectSystem.FileSystemMirroring.dll",
        };

        private RPackageTestCompositionCatalog() :
            base(_rPackageAssemblies) {
        }
    }
}

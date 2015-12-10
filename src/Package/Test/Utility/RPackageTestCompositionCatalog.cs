using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.Editor.Mocks;

namespace Microsoft.VisualStudio.R.Package.Test.Utility {
    [ExcludeFromCodeCoverage]
    public sealed class RPackageTestCompositionCatalog : TestCompositionCatalog {
        private static Lazy<RPackageTestCompositionCatalog> _instance = Lazy.Create(() => new RPackageTestCompositionCatalog());

        public static ITestCompositionCatalog Current => _instance.Value;

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

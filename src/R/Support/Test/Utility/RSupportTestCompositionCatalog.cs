using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core;
using Microsoft.VisualStudio.Editor.Mocks;

namespace Microsoft.R.Support.Test.Utility {
    [ExcludeFromCodeCoverage]
    public sealed class RSupportTestCompositionCatalog : TestCompositionCatalog {
        private static Lazy<RSupportTestCompositionCatalog> _instance = Lazy.Create(() => new RSupportTestCompositionCatalog());

        public static ITestCompositionCatalog Current => _instance.Value;

        private static string[] _rEditorAssemblies = new string[]
        {
            "Microsoft.R.Support.dll",
            "Microsoft.R.Host.Client.dll",
            "Microsoft.R.Common.Core.dll",
        };

        private RSupportTestCompositionCatalog() :
            base(_rEditorAssemblies) {
        }
    }
}

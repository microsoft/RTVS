using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Editor.Mocks;

namespace Microsoft.R.Support.Test.Utility {
    [ExcludeFromCodeCoverage]
    public sealed class RSupportTestCompositionCatalog : TestCompositionCatalog {
        private static RSupportTestCompositionCatalog _instance;

        public static ITestCompositionCatalog Current {
            get {
                if (_instance == null) {
                    _instance = new RSupportTestCompositionCatalog();
                }

                return _instance;
            }
        }

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

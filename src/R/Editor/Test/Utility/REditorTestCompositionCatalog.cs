using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Editor.Mocks;

namespace Microsoft.R.Editor.Test.Utility
{
    [ExcludeFromCodeCoverage]
    public sealed class REditorTestCompositionCatalog : TestCompositionCatalog
    {
        private static REditorTestCompositionCatalog _instance;

        public static ITestCompositionCatalog Current
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new REditorTestCompositionCatalog();
                }

                return _instance;
            }
        }

        private static string[] _rEditorAssemblies = new string[]
        {
            "Microsoft.Markdown.Editor.dll",
            "Microsoft.R.Editor.dll",
            "Microsoft.R.Editor.Test.dll",
            "Microsoft.R.Support.dll",
            "Microsoft.R.Support.Test.dll",
        };

        private REditorTestCompositionCatalog() :
            base(_rEditorAssemblies)
        {
        }
    }
}

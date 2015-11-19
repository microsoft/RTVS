using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Editor.Mocks;

namespace Microsoft.Markdown.Editor.Test.Utility
{
    [ExcludeFromCodeCoverage]
    public sealed class MarkdownTestCompositionCatalog : TestCompositionCatalog
    {
        private static MarkdownTestCompositionCatalog _instance;

        public static ITestCompositionCatalog Current
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MarkdownTestCompositionCatalog();
                }

                return _instance;
            }
        }

        private static string[] _rEditorAssemblies = new string[]
        {
            "Microsoft.Markdown.Editor.dll",
            "Microsoft.R.Support.dll",
            "Microsoft.R.Editor.dll",
        };

        private MarkdownTestCompositionCatalog() :
            base(_rEditorAssemblies)
        {
        }
    }
}

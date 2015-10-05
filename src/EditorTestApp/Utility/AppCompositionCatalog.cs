using Microsoft.VisualStudio.Editor.Mocks;

namespace Microsoft.Languages.Editor.Application.Utility
{
    public sealed class AppCompositionCatalog : TestCompositionCatalog
    {
        private static AppCompositionCatalog _instance;

        public static ITestCompositionCatalog Current
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AppCompositionCatalog();
                }

                return _instance;
            }
        }

        private static string[] _rEditorAssemblies = new string[]
        {
            "Microsoft.Markdown.Editor.dll",
            "Microsoft.R.Editor.dll",
            "Microsoft.R.Support.dll",
            "Microsoft.R.Support.Test.dll",
            "Microsoft.Languages.Editor.dll",
            "Microsoft.Languages.Editor.Application.dll"
        };

        private AppCompositionCatalog() :
            base(_rEditorAssemblies)
        {
        }
    }
}

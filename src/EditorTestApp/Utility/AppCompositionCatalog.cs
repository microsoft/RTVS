using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Editor.Mocks;

namespace Microsoft.Languages.Editor.Application.Utility
{
    [ExcludeFromCodeCoverage]
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
            "Microsoft.R.Host.Client.dll",
            "Microsoft.R.Common.Core.dll",
            "Microsoft.Languages.Editor.dll",
            "Microsoft.Languages.Editor.Application.dll"
        };

        private AppCompositionCatalog() :
            base(_rEditorAssemblies)
        {
        }
    }
}

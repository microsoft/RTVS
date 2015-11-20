using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.Editor.Mocks;

namespace Microsoft.Languages.Editor.Test.Utility
{
    [ExcludeFromCodeCoverage]
    public sealed class EditorTestCompositionCatalog: TestCompositionCatalog
    {
        private static EditorTestCompositionCatalog _instance;

        public static ITestCompositionCatalog Current
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new EditorTestCompositionCatalog();
                }

                return _instance;
            }
        }

        private EditorTestCompositionCatalog():
            base(new string[0])
        {
        }
    }
}

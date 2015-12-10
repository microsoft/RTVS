using System.Diagnostics.CodeAnalysis;
using Microsoft.Languages.Core.Test.Utility;
using Microsoft.Languages.Editor.Tests.Shell;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Languages.Editor.Test.Utility {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TestFilesSetup
    {
        static object _deploymentLock = new object();
        static bool _deployed = false;

        [AssemblyInitialize]
        public static void DeployFiles(TestContext context)
        {
            lock (_deploymentLock)
            {
                if (!_deployed)
                {
                    _deployed = true;
                    TestEditorShell.Create(EditorTestCompositionCatalog.Current);

                    string srcFilesFolder;
                    string testFilesDir;

                    TestSetup.GetTestFolders(@"Common\Editor\Test\Files", CommonTestData.TestFilesRelativePath, context, out srcFilesFolder, out testFilesDir);
                    TestSetup.CopyDirectory(srcFilesFolder, testFilesDir);
                }
            }
        }
    }
}

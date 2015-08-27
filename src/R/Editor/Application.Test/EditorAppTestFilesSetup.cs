using Microsoft.Languages.Core.Test.Utility;
using Microsoft.R.Editor.Test.Utility;
using Microsoft.R.Support.Test.RD.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.R.Editor.Application.Test
{
    [TestClass]
    public class EditorAppTestFilesSetup
    {
        static object _deploymentLock = new object();
        static bool _deployed = false;

        [AssemblyInitialize]
        public static void DeployFiles(TestContext context)
        {
            RdTestFilesSetup.DeployFiles(context);
            EditorTestFilesSetup.DeployFiles(context);

            lock (_deploymentLock)
            {
                if (!_deployed)
                {
                    _deployed = true;

                    string srcFilesFolder;
                    string testFilesDir;

                    TestSetup.GetTestFolders(@"R\Editor\Application.Test\Files", CommonTestData.TestFilesRelativePath, context, out srcFilesFolder, out testFilesDir);
                    TestSetup.CopyDirectory(srcFilesFolder, testFilesDir);
                }
            }
        }
    }
}

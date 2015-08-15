using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Languages.Core.Test.Utility;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Languages.Core.Test.Utility
{
    [ExcludeFromCodeCoverage]
    public static class CommonTestData
    {
        public const string TestFilesRelativePath = @"Files\";
    }

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

                     string srcFilesFolder;
                    string testFilesDir;

                    TestSetup.GetTestFolders(@"Common\Core\Test\Files", CommonTestData.TestFilesRelativePath, context, out srcFilesFolder, out testFilesDir);
                    TestSetup.CopyDirectory(srcFilesFolder, testFilesDir);
                }
            }
        }
    }
}

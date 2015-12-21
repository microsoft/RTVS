using System.Diagnostics.CodeAnalysis;
using Microsoft.Common.Core.Test.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.VisualStudio.R.Application.Test.Utility {
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TestFilesSetup {
        static object _deploymentLock = new object();
        static bool _deployed = false;

        [AssemblyInitialize]
        public static void DeployFiles(TestContext context) {
            lock (_deploymentLock) {
                if (!_deployed) {
                    _deployed = true;

                    string srcFilesFolder;
                    string testFilesDir;

                    TestSetupUtilities.GetTestFolders(@"Package\TestApp\Files", CommonTestData.TestFilesRelativePath, context, out srcFilesFolder, out testFilesDir);
                    TestSetupUtilities.CopyDirectory(srcFilesFolder, testFilesDir);
                }
            }
        }
    }
}
